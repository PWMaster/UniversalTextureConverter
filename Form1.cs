using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Text;

namespace MyTextureConverter
{
    public partial class Form1 : Form
    {
        private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.ini");
        private static readonly string LegacySettingsPath = Path.Combine(AppContext.BaseDirectory, "path.txt");
        private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "log.txt");

        private static readonly int[] UpscaleSizes = { 64, 128, 256, 512, 1024 };
        private static readonly int[] DownscaleSizes = { 256, 512, 1024, 2048, 4096 };

        private readonly string[] _startupPaths;
        private readonly Stopwatch _runTimer = new();
        private CancellationTokenSource? _runCts;
        private LastRun? _lastRun;
        private long _ignoreClicksUntil;
        private bool _closeAfterRun;

        private readonly record struct Job(string Source, string Target);
        private readonly record struct ProgressInfo(int Done, int Total, string File);
        private sealed record JobList(List<Job> Jobs, int Duplicates);
        private sealed record ErrorEntry(string? Path, string Text)
        {
            public override string ToString() => Text;
        }
        private sealed record ConversionOptions(string? ExportDir, bool Overwrite, bool OnlyChanged, int Threads, TextureOptions Texture);
        private sealed record ConversionResult(int Total, int Converted, int Skipped, int Failed, int Duplicates, bool Cancelled,
            IReadOnlyList<Job> FailedJobs);
        private sealed record LastRun(ConversionOptions Options, IReadOnlyList<Job> FailedJobs);

        public Form1(string[] startupPaths)
        {
            InitializeComponent();
            MinimumSize = Size;
            _startupPaths = startupPaths;

            Text = $"Texture Converter v{Application.ProductVersion} by @PW_Master_1";
            Status.Text = "";

            FormatComboBox.Items.AddRange(OutputFormat.All.ToArray<object>());
            FormatComboBox.SelectedIndex = 0;
            UpscaleSizeComboBox.Items.AddRange(UpscaleSizes.Cast<object>().ToArray());
            UpscaleSizeComboBox.SelectedItem = 256;
            DownscaleSizeComboBox.Items.AddRange(DownscaleSizes.Cast<object>().ToArray());
            DownscaleSizeComboBox.SelectedItem = 1024;
            ThreadsUpDown.Maximum = Environment.ProcessorCount;
            ThreadsUpDown.Value = Environment.ProcessorCount;
            MipmapsCheckBox.Checked = true;

            foreach (InputFormat format in InputFormat.All)
            {
                var item = new ToolStripMenuItem(format.Name) { CheckOnClick = true, Checked = true, Tag = format };
                item.CheckedChanged += (_, _) => UpdateInputFormatsText();
                InputFormatsMenu.Items.Add(item);
            }
            InputFormatsMenu.Items.Add(new ToolStripSeparator());
            InputFormatsMenu.Items.Add("Выбрать все", null, (_, _) => SetAllInputFormats(true));
            InputFormatsMenu.Items.Add("Снять все", null, (_, _) => SetAllInputFormats(false));

            AllowDrop = true;
            DragEnter += Files_DragEnter;
            DragDrop += Files_DragDrop;
            foreach (Control control in Controls)
            {
                if (control == ImportBox || control == ExportBox)
                    continue;
                control.AllowDrop = true;
                control.DragEnter += Files_DragEnter;
                control.DragDrop += Files_DragDrop;
            }

            LoadSettings();
            UpdateInputFormatsText();
            UpdateOptionStates();
            ActiveControl = ConvertButton;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_startupPaths.Length > 0)
                ConvertPaths(_startupPaths);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (_runCts == null)
                Taskbar.SetState(this, TaskbarState.None);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_runCts != null)
            {
                e.Cancel = true;
                _closeAfterRun = true;
                CancelRun();
                return;
            }

            SaveSettings();
            base.OnFormClosing(e);
        }

        private void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    if (File.Exists(LegacySettingsPath))
                    {
                        string[] legacy = File.ReadAllLines(LegacySettingsPath);
                        if (legacy.Length > 0)
                            ImportBox.Text = legacy[0];
                        if (legacy.Length > 1)
                            ExportBox.Text = legacy[1];
                    }
                    return;
                }

                foreach (string line in File.ReadAllLines(SettingsPath))
                {
                    int separator = line.IndexOf('=');
                    if (separator <= 0)
                        continue;

                    string key = line[..separator].Trim();
                    string value = line[(separator + 1)..].Trim();
                    bool flag = value == bool.TrueString;
                    int.TryParse(value, out int number);

                    switch (key)
                    {
                        case "Import": ImportBox.Text = value; break;
                        case "Export": ExportBox.Text = value; break;
                        case "Format":
                            if (OutputFormat.All.FirstOrDefault(f => f.Name == value) is OutputFormat format)
                                FormatComboBox.SelectedItem = format;
                            break;
                        case "InputFormats":
                            var names = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToHashSet();
                            foreach (ToolStripMenuItem item in InputFormatItems)
                                item.Checked = names.Contains(item.Text!);
                            break;
                        case "Overwrite": OverrideCheckBox.Checked = flag; break;
                        case "OnlyChanged": OnlyChangedCheckBox.Checked = flag; break;
                        case "UpscaleDds": UpscaleCheckBox.Checked = flag; break;
                        case "UpscaleSize": if (UpscaleSizes.Contains(number)) UpscaleSizeComboBox.SelectedItem = number; break;
                        case "Downscale": DownscaleCheckBox.Checked = flag; break;
                        case "DownscaleSize": if (DownscaleSizes.Contains(number)) DownscaleSizeComboBox.SelectedItem = number; break;
                        case "Mipmaps": MipmapsCheckBox.Checked = flag; break;
                        case "Quality": if (number is >= 1 and <= 100) QualityUpDown.Value = number; break;
                        case "Threads": if (number >= 1) ThreadsUpDown.Value = Math.Min(number, (int)ThreadsUpDown.Maximum); break;
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }

        private void SaveSettings()
        {
            try
            {
                File.WriteAllLines(SettingsPath, new[]
                {
                    "Import=" + ImportBox.Text.Trim(),
                    "Export=" + ExportBox.Text.Trim(),
                    "Format=" + SelectedFormat.Name,
                    "InputFormats=" + string.Join(",", InputFormatItems.Where(i => i.Checked).Select(i => i.Text)),
                    "Overwrite=" + OverrideCheckBox.Checked,
                    "OnlyChanged=" + OnlyChangedCheckBox.Checked,
                    "UpscaleDds=" + UpscaleCheckBox.Checked,
                    "UpscaleSize=" + UpscaleSizeComboBox.SelectedItem,
                    "Downscale=" + DownscaleCheckBox.Checked,
                    "DownscaleSize=" + DownscaleSizeComboBox.SelectedItem,
                    "Mipmaps=" + MipmapsCheckBox.Checked,
                    "Quality=" + QualityUpDown.Value,
                    "Threads=" + ThreadsUpDown.Value
                });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
            }
        }

        private OutputFormat SelectedFormat => (OutputFormat)FormatComboBox.SelectedItem!;

        private IEnumerable<ToolStripMenuItem> InputFormatItems =>
            InputFormatsMenu.Items.OfType<ToolStripMenuItem>().Where(i => i.Tag is InputFormat);

        private HashSet<string> SelectedInputExtensions() => InputFormatItems
            .Where(i => i.Checked)
            .SelectMany(i => ((InputFormat)i.Tag!).Extensions)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        private void SetAllInputFormats(bool value)
        {
            foreach (ToolStripMenuItem item in InputFormatItems)
                item.Checked = value;
        }

        private void UpdateInputFormatsText()
        {
            var selected = InputFormatItems.Where(i => i.Checked).Select(i => i.Text).ToList();
            string text = selected.Count == InputFormat.All.Count ? "все"
                : selected.Count == 0 ? "не выбраны"
                : string.Join(", ", selected);
            InputFormatsButton.Text = "Входные: " + text + " ▾";
        }

        private void InputFormatsButton_Click(object sender, EventArgs e)
        {
            InputFormatsMenu.Show(InputFormatsButton, 0, InputFormatsButton.Height);
        }

        private void InputFormatsMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                e.Cancel = true;
        }

        private void Option_Changed(object sender, EventArgs e) => UpdateOptionStates();

        private void UpdateOptionStates()
        {
            bool idle = _runCts == null;
            bool cancelling = _runCts?.IsCancellationRequested == true;

            foreach (Control control in new Control[]
            {
                ImportBox, ImportButton, ExportBox, ExportButton, OnlyChangedCheckBox, FormatComboBox,
                InputFormatsButton, UpscaleCheckBox, DownscaleCheckBox, ThreadsUpDown
            })
            {
                control.Enabled = idle;
            }

            OverrideCheckBox.Enabled = idle && !OnlyChangedCheckBox.Checked;
            MipmapsCheckBox.Enabled = idle && SelectedFormat.DdsFormat != null;
            QualityUpDown.Enabled = idle && SelectedFormat.SupportsQuality;
            UpscaleSizeComboBox.Enabled = idle && UpscaleCheckBox.Checked;
            DownscaleSizeComboBox.Enabled = idle && DownscaleCheckBox.Checked;
            RetryButton.Enabled = idle && _lastRun?.FailedJobs.Count > 0;

            ConvertButton.Enabled = !cancelling;
            ConvertButton.Text = idle ? "Конвертировать" : cancelling ? "Отмена..." : "Отмена";
        }

        private ConversionOptions CreateOptions(string? exportDir) => new(
            exportDir,
            OverrideCheckBox.Checked,
            OnlyChangedCheckBox.Checked,
            (int)ThreadsUpDown.Value,
            new TextureOptions(
                SelectedFormat,
                UpscaleCheckBox.Checked ? (int)UpscaleSizeComboBox.SelectedItem! : null,
                DownscaleCheckBox.Checked ? (int)DownscaleSizeComboBox.SelectedItem! : null,
                MipmapsCheckBox.Checked,
                (int)QualityUpDown.Value));

        private static string DescribeOptions(ConversionOptions options)
        {
            TextureOptions texture = options.Texture;
            return $"Экспорт: {options.ExportDir ?? "рядом с исходниками"}{Environment.NewLine}" +
                $"Формат: {texture.Format.Name}, перезапись: {options.Overwrite}, только изменённые: {options.OnlyChanged}, " +
                $"увеличение DDS: {texture.UpscaleDdsTo?.ToString() ?? "нет"}, макс. размер: {texture.MaxSize?.ToString() ?? "нет"}, " +
                $"мипмапы: {texture.GenerateMipmaps}, качество: {texture.Quality}, потоков: {options.Threads}";
        }

        private void Import_Click(object sender, EventArgs e)
        {
            string? path = PickFolder("Выберите папку для импорта", ImportBox.Text);
            if (path != null)
                ImportBox.Text = path;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            string? path = PickFolder("Выберите папку для сохранения", ExportBox.Text);
            if (path != null)
                ExportBox.Text = path;
        }

        private static string? PickFolder(string description, string current)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = description,
                UseDescriptionForTitle = true,
                SelectedPath = Directory.Exists(current.Trim()) ? current.Trim() : ""
            };
            return fbd.ShowDialog() == DialogResult.OK ? fbd.SelectedPath : null;
        }

        private void PathBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = GetDroppedFolder(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void PathBox_DragDrop(object sender, DragEventArgs e)
        {
            if (sender is TextBox box && GetDroppedFolder(e) is string folder)
                box.Text = folder;
        }

        private static string[]? GetDroppedPaths(DragEventArgs e) =>
            e.Data?.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } paths ? paths : null;

        private static string? GetDroppedFolder(DragEventArgs e)
        {
            if (GetDroppedPaths(e) is not string[] paths)
                return null;
            return Directory.Exists(paths[0]) ? paths[0] : Path.GetDirectoryName(paths[0]);
        }

        private void Files_DragEnter(object? sender, DragEventArgs e)
        {
            e.Effect = _runCts == null && GetDroppedPaths(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Files_DragDrop(object? sender, DragEventArgs e)
        {
            if (GetDroppedPaths(e) is string[] paths)
                ConvertPaths(paths);
        }

        private void OpenLog_Click(object sender, EventArgs e)
        {
            if (File.Exists(LogPath))
                OpenWithShell(LogPath);
            else
                Status.Text = "Лог ещё не создан";
        }

        private void OpenWithShell(string path)
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Win32Exception ex)
            {
                Status.Text = "Не удалось открыть: " + ex.Message;
            }
        }

        private static void ShowInExplorer(string path)
        {
            if (File.Exists(path))
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            else if (Path.GetDirectoryName(path) is string dir && Directory.Exists(dir))
                Process.Start("explorer.exe", $"\"{dir}\"");
        }

        private ErrorEntry? SelectedError => ErrorListBox.SelectedItem as ErrorEntry;

        private void ErrorListBox_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedError is { Path: string path })
                ShowInExplorer(path);
        }

        private void ErrorListBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            int index = ErrorListBox.IndexFromPoint(e.Location);
            if (index >= 0)
                ErrorListBox.SelectedIndex = index;
        }

        private void ErrorMenu_Opening(object sender, CancelEventArgs e)
        {
            if (ErrorListBox.Items.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            ErrorEntry? entry = SelectedError;
            OpenFileMenuItem.Enabled = entry?.Path != null && File.Exists(entry.Path);
            ShowInExplorerMenuItem.Enabled = entry?.Path != null;
            CopyMenuItem.Enabled = entry != null;
        }

        private void OpenFileMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedError is { Path: string path })
                OpenWithShell(path);
        }

        private void ShowInExplorerMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedError is { Path: string path })
                ShowInExplorer(path);
        }

        private void CopyMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedError is ErrorEntry entry)
                Clipboard.SetText(entry.Text);
        }

        private void CopyAllMenuItem_Click(object sender, EventArgs e)
        {
            if (ErrorListBox.Items.Count > 0)
                Clipboard.SetText(string.Join(Environment.NewLine, ErrorListBox.Items.Cast<object>()));
        }

        private async void Convert_Click(object sender, EventArgs e)
        {
            if (_runCts != null)
            {
                CancelRun();
                return;
            }
            if (Environment.TickCount64 < _ignoreClicksUntil)
                return;

            string? importDir = NormalizeDir(ImportBox.Text);
            string? exportDir = NormalizeDir(ExportBox.Text);
            HashSet<string> extensions = SelectedInputExtensions();

            if (importDir == null || !Directory.Exists(importDir))
            {
                Status.Text = "Неверный путь импорта";
                return;
            }
            if (exportDir == null)
            {
                Status.Text = "Неверный путь экспорта";
                return;
            }
            if (extensions.Count == 0)
            {
                Status.Text = "Не выбраны входные форматы";
                return;
            }

            ConversionOptions options = CreateOptions(exportDir);
            await RunAsync(options, "Импорт: " + importDir,
                (log, token) => BuildJobs(EnumerateFolder(importDir, exportDir, extensions, options.Texture.Format, token), log));
        }

        private void CancelRun()
        {
            _runCts!.Cancel();
            _ignoreClicksUntil = Environment.TickCount64 + 1000;
            UpdateOptionStates();
            Status.Text = "Отмена: ожидание завершения текущих файлов...";
        }

        private async void Retry_Click(object sender, EventArgs e)
        {
            if (_runCts != null || _lastRun is not { FailedJobs.Count: > 0 } last)
                return;

            ConversionOptions options = last.Options with { Overwrite = true, OnlyChanged = false };
            var jobs = last.FailedJobs.ToList();
            await RunAsync(options, $"Повтор ошибочных: {jobs.Count}", (_, _) => new JobList(jobs, 0));
        }

        private async void ConvertPaths(string[] paths)
        {
            if (_runCts != null)
                return;

            string? exportDir = NormalizeDir(ExportBox.Text);
            HashSet<string> extensions = SelectedInputExtensions();
            ConversionOptions options = CreateOptions(exportDir);
            await RunAsync(options, "Перетащено: " + string.Join("; ", paths),
                (log, token) => BuildJobs(EnumerateDropped(paths, exportDir, extensions, options.Texture.Format, token), log));
        }

        private async Task RunAsync(ConversionOptions options, string header, Func<TextWriter, CancellationToken, JobList> collect)
        {
            ErrorListBox.Items.Clear();
            var progress = new Progress<ProgressInfo>(OnProgress);
            var errors = new Progress<ErrorEntry>(OnError);

            using var cts = new CancellationTokenSource();
            _runCts = cts;
            UpdateOptionStates();
            Status.Text = "Поиск текстур...";
            Progress.Value = 0;
            Taskbar.SetState(this, TaskbarState.Indeterminate);
            _runTimer.Restart();

            TextWriter log = OpenLog();
            try
            {
                log.WriteLine($"Конвертация {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                log.WriteLine(header);
                log.WriteLine(DescribeOptions(options));
                log.WriteLine();

                ConversionResult result = await Task.Run(() =>
                    RunJobs(token => collect(log, token), options, progress, errors, log, cts.Token));
                _runTimer.Stop();
                _lastRun = new LastRun(options, result.FailedJobs);

                string summary = (result.Cancelled ? "Отменено" : "Готово") + $" за {FormatTime(_runTimer.Elapsed)}\n" +
                    $"Экспортировано: {result.Converted}/{result.Total}  Пропущено: {result.Skipped}  " +
                    $"Ошибок: {result.Failed}  Дубликатов: {result.Duplicates}";

                log.WriteLine();
                log.WriteLine(summary.Replace("\n", Environment.NewLine));

                if (IsDisposed)
                    return;

                Progress.Value = 0;
                Status.Text = summary;
                NotifyFinished(result);
            }
            catch (Exception ex)
            {
                log.WriteLine("ОШИБКА: " + ex);
                if (!IsDisposed)
                {
                    Status.Text = "Ошибка: " + ex.Message;
                    Taskbar.SetState(this, TaskbarState.Error);
                    SystemSounds.Hand.Play();
                }
            }
            finally
            {
                log.Dispose();
                _runCts = null;
                if (!IsDisposed)
                {
                    UpdateOptionStates();
                    if (_closeAfterRun)
                        BeginInvoke(Close);
                }
            }
        }

        private void NotifyFinished(ConversionResult result)
        {
            if (result.Failed > 0)
            {
                Taskbar.SetValue(this, 1, 1);
                Taskbar.SetState(this, TaskbarState.Error);
            }
            else
            {
                Taskbar.SetState(this, TaskbarState.None);
            }

            if (result.Cancelled)
                return;

            (result.Failed > 0 ? SystemSounds.Exclamation : SystemSounds.Asterisk).Play();
            if (ActiveForm != this)
                Taskbar.Flash(this);
        }

        private TextWriter OpenLog()
        {
            try
            {
                return TextWriter.Synchronized(new StreamWriter(LogPath, false, new UTF8Encoding(true)));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                ErrorListBox.Items.Add(new ErrorEntry(null, $"Не удалось открыть лог {LogPath}: {ex.Message}"));
                return TextWriter.Null;
            }
        }

        private void OnProgress(ProgressInfo info)
        {
            if (IsDisposed || _runCts == null || _runCts.IsCancellationRequested)
                return;

            Progress.Maximum = Math.Max(info.Total, 1);
            Progress.Value = Math.Min(info.Done, Progress.Maximum);
            Taskbar.SetValue(this, info.Done, info.Total);

            TimeSpan elapsed = _runTimer.Elapsed;
            string eta = info.Done > 0 && info.Done < info.Total
                ? " · осталось ~" + FormatTime(elapsed * (info.Total - info.Done) / info.Done)
                : "";
            Status.Text = $"Экспорт: {info.Done}/{info.Total} · прошло {FormatTime(elapsed)}{eta}\n{info.File}";
        }

        private void OnError(ErrorEntry error)
        {
            if (!IsDisposed)
                ErrorListBox.Items.Add(error);
        }

        private static string FormatTime(TimeSpan time) =>
            time.TotalHours >= 1 ? time.ToString(@"h\:mm\:ss") : time.ToString(@"m\:ss");

        private static string? NormalizeDir(string text)
        {
            string path = text.Trim().Trim('"');
            if (path.Length == 0)
                return null;

            try
            {
                if (!Path.IsPathFullyQualified(path))
                    return null;
                return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return null;
            }
        }

        private static bool IsSubDirectory(string path, string parent)
        {
            string relative = Path.GetRelativePath(parent, path);
            return relative != "." && relative != ".." &&
                   !relative.StartsWith(".." + Path.DirectorySeparatorChar) &&
                   !Path.IsPathRooted(relative);
        }

        private static IEnumerable<Job> EnumerateFolder(string importDir, string? exportDir, HashSet<string> extensions,
            OutputFormat format, CancellationToken token)
        {
            string targetRoot = exportDir ?? importDir;
            bool skipExportDir = exportDir != null && IsSubDirectory(exportDir, importDir);
            var enumeration = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true };

            foreach (string source in Directory.EnumerateFiles(importDir, "*", enumeration))
            {
                token.ThrowIfCancellationRequested();

                if (!extensions.Contains(Path.GetExtension(source)))
                    continue;
                if (skipExportDir && IsSubDirectory(source, exportDir!))
                    continue;

                yield return new Job(source,
                    Path.ChangeExtension(Path.Combine(targetRoot, Path.GetRelativePath(importDir, source)), format.Extension));
            }
        }

        private static IEnumerable<Job> EnumerateDropped(string[] paths, string? exportDir, HashSet<string> extensions,
            OutputFormat format, CancellationToken token)
        {
            foreach (string path in paths)
            {
                token.ThrowIfCancellationRequested();

                if (Directory.Exists(path))
                {
                    string dir = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
                    string? target = exportDir == null ? null : Path.Combine(exportDir, Path.GetFileName(dir));
                    foreach (Job job in EnumerateFolder(dir, target, extensions, format, token))
                        yield return job;
                }
                else if (File.Exists(path) && InputFormat.PriorityByExtension.ContainsKey(Path.GetExtension(path)))
                {
                    string targetDir = exportDir ?? Path.GetDirectoryName(path)!;
                    yield return new Job(path, Path.Combine(targetDir, Path.GetFileNameWithoutExtension(path) + format.Extension));
                }
            }
        }

        private static JobList BuildJobs(IEnumerable<Job> candidates, TextWriter log)
        {
            var groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();

            foreach (Job candidate in candidates)
            {
                if (string.Equals(candidate.Source, candidate.Target, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!groups.TryGetValue(candidate.Target, out List<string>? sources))
                {
                    sources = new List<string>();
                    groups.Add(candidate.Target, sources);
                    order.Add(candidate.Target);
                }
                sources.Add(candidate.Source);
            }

            int duplicates = 0;
            var jobs = new List<Job>(order.Count);
            foreach (string target in order)
            {
                List<string> sources = groups[target];
                string chosen = sources.MinBy(s => InputFormat.PriorityByExtension[Path.GetExtension(s)])!;
                jobs.Add(new Job(chosen, target));

                foreach (string other in sources)
                {
                    if (other == chosen)
                        continue;
                    duplicates++;
                    log.WriteLine($"ДУБЛИКАТ  {other} (используется {Path.GetFileName(chosen)})");
                }
            }
            return new JobList(jobs, duplicates);
        }

        private static string? GetSkipReason(Job job, ConversionOptions options)
        {
            if (!File.Exists(job.Target))
                return null;
            if (options.OnlyChanged)
                return File.GetLastWriteTimeUtc(job.Source) > File.GetLastWriteTimeUtc(job.Target) ? null : "не изменён";
            return options.Overwrite ? null : "уже существует";
        }

        private static string GetDisplayPath(string target, string? exportDir) =>
            exportDir != null && IsSubDirectory(target, exportDir) ? Path.GetRelativePath(exportDir, target) : target;

        private static ConversionResult RunJobs(Func<CancellationToken, JobList> collect, ConversionOptions options,
            IProgress<ProgressInfo> progress, IProgress<ErrorEntry> errors, TextWriter log, CancellationToken token)
        {
            int total = 0, done = 0, converted = 0, skipped = 0, failed = 0, duplicates = 0;
            var failedJobs = new ConcurrentBag<Job>();
            try
            {
                JobList list = collect(token);
                total = list.Jobs.Count;
                duplicates = list.Duplicates;
                progress.Report(new ProgressInfo(0, total, ""));

                var parallel = new ParallelOptions { CancellationToken = token, MaxDegreeOfParallelism = options.Threads };
                Parallel.ForEach(list.Jobs, parallel, job =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    if (GetSkipReason(job, options) is string reason)
                    {
                        Interlocked.Increment(ref skipped);
                        log.WriteLine($"ПРОПУЩЕН {job.Target} ({reason})");
                    }
                    else
                    {
                        try
                        {
                            TextureConverter.Convert(job.Source, job.Target, options.Texture);
                            Interlocked.Increment(ref converted);
                            log.WriteLine($"OK       {job.Source} -> {job.Target}");
                        }
                        catch (Exception ex)
                        {
                            string error = $"{job.Source}: {ex.Message}";
                            Interlocked.Increment(ref failed);
                            failedJobs.Add(job);
                            log.WriteLine($"ОШИБКА   {error}");
                            errors.Report(new ErrorEntry(job.Source, error));
                        }
                    }

                    progress.Report(new ProgressInfo(Interlocked.Increment(ref done), total,
                        GetDisplayPath(job.Target, options.ExportDir)));
                });
            }
            catch (OperationCanceledException)
            {
                return new ConversionResult(total, converted, skipped, failed, duplicates, true, failedJobs.ToList());
            }

            return new ConversionResult(total, converted, skipped, failed, duplicates, false, failedJobs.ToList());
        }
    }
}
