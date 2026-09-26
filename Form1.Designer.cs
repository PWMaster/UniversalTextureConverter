namespace MyTextureConverter
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            ImportBox = new TextBox();
            ImportButton = new Button();
            ExportBox = new TextBox();
            ExportButton = new Button();
            OverrideCheckBox = new CheckBox();
            OnlyChangedCheckBox = new CheckBox();
            FormatLabel = new Label();
            FormatComboBox = new ComboBox();
            InputFormatsButton = new Button();
            QualityLabel = new Label();
            QualityUpDown = new NumericUpDown();
            UpscaleCheckBox = new CheckBox();
            UpscaleSizeComboBox = new ComboBox();
            ThreadsLabel = new Label();
            ThreadsUpDown = new NumericUpDown();
            DownscaleCheckBox = new CheckBox();
            DownscaleSizeComboBox = new ComboBox();
            MipmapsCheckBox = new CheckBox();
            ConvertButton = new Button();
            RetryButton = new Button();
            OpenLogButton = new Button();
            Status = new Label();
            Progress = new ProgressBar();
            ErrorListBox = new ListBox();
            ErrorMenu = new ContextMenuStrip(components);
            OpenFileMenuItem = new ToolStripMenuItem();
            ShowInExplorerMenuItem = new ToolStripMenuItem();
            ErrorMenuSeparator = new ToolStripSeparator();
            CopyMenuItem = new ToolStripMenuItem();
            CopyAllMenuItem = new ToolStripMenuItem();
            InputFormatsMenu = new ContextMenuStrip(components);
            ToolTip = new ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)QualityUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ThreadsUpDown).BeginInit();
            ErrorMenu.SuspendLayout();
            SuspendLayout();
            //
            // ImportBox
            //
            ImportBox.AllowDrop = true;
            ImportBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ImportBox.Location = new Point(12, 16);
            ImportBox.Margin = new Padding(4, 3, 4, 3);
            ImportBox.Name = "ImportBox";
            ImportBox.PlaceholderText = "Путь импорта (можно перетащить папку)";
            ImportBox.Size = new Size(446, 23);
            ImportBox.TabIndex = 0;
            ImportBox.TextAlign = HorizontalAlignment.Center;
            ImportBox.DragDrop += PathBox_DragDrop;
            ImportBox.DragEnter += PathBox_DragEnter;
            //
            // ImportButton
            //
            ImportButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ImportButton.Location = new Point(462, 15);
            ImportButton.Margin = new Padding(4, 3, 4, 3);
            ImportButton.Name = "ImportButton";
            ImportButton.Size = new Size(59, 25);
            ImportButton.TabIndex = 1;
            ImportButton.Text = "...";
            ImportButton.UseVisualStyleBackColor = true;
            ImportButton.Click += Import_Click;
            //
            // ExportBox
            //
            ExportBox.AllowDrop = true;
            ExportBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ExportBox.Location = new Point(12, 46);
            ExportBox.Margin = new Padding(4, 3, 4, 3);
            ExportBox.Name = "ExportBox";
            ExportBox.PlaceholderText = "Путь экспорта (можно перетащить папку)";
            ExportBox.Size = new Size(446, 23);
            ExportBox.TabIndex = 2;
            ExportBox.TextAlign = HorizontalAlignment.Center;
            ExportBox.DragDrop += PathBox_DragDrop;
            ExportBox.DragEnter += PathBox_DragEnter;
            //
            // ExportButton
            //
            ExportButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ExportButton.Location = new Point(462, 45);
            ExportButton.Margin = new Padding(4, 3, 4, 3);
            ExportButton.Name = "ExportButton";
            ExportButton.Size = new Size(59, 25);
            ExportButton.TabIndex = 3;
            ExportButton.Text = "...";
            ExportButton.UseVisualStyleBackColor = true;
            ExportButton.Click += Save_Click;
            //
            // OverrideCheckBox
            //
            OverrideCheckBox.Location = new Point(14, 78);
            OverrideCheckBox.Name = "OverrideCheckBox";
            OverrideCheckBox.Size = new Size(110, 20);
            OverrideCheckBox.TabIndex = 4;
            OverrideCheckBox.Text = "Перезаписать";
            ToolTip.SetToolTip(OverrideCheckBox, "Конвертировать заново, даже если результат уже существует");
            //
            // OnlyChangedCheckBox
            //
            OnlyChangedCheckBox.Location = new Point(130, 78);
            OnlyChangedCheckBox.Name = "OnlyChangedCheckBox";
            OnlyChangedCheckBox.Size = new Size(150, 20);
            OnlyChangedCheckBox.TabIndex = 5;
            OnlyChangedCheckBox.Text = "Только изменённые";
            ToolTip.SetToolTip(OnlyChangedCheckBox, "Конвертировать только новые файлы и те, у которых исходник новее результата");
            OnlyChangedCheckBox.CheckedChanged += Option_Changed;
            //
            // FormatLabel
            //
            FormatLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FormatLabel.AutoSize = true;
            FormatLabel.Location = new Point(355, 80);
            FormatLabel.Name = "FormatLabel";
            FormatLabel.Size = new Size(49, 15);
            FormatLabel.TabIndex = 6;
            FormatLabel.Text = "Формат:";
            //
            // FormatComboBox
            //
            FormatComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            FormatComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            FormatComboBox.Location = new Point(415, 76);
            FormatComboBox.Name = "FormatComboBox";
            FormatComboBox.Size = new Size(106, 23);
            FormatComboBox.TabIndex = 7;
            ToolTip.SetToolTip(FormatComboBox, "Формат результата. BC7 и sRGB поддерживаются только движками с DirectX 11+");
            FormatComboBox.SelectedIndexChanged += Option_Changed;
            //
            // InputFormatsButton
            //
            InputFormatsButton.Location = new Point(12, 104);
            InputFormatsButton.Name = "InputFormatsButton";
            InputFormatsButton.Size = new Size(268, 24);
            InputFormatsButton.TabIndex = 8;
            InputFormatsButton.Text = "Входные: все ▾";
            InputFormatsButton.TextAlign = ContentAlignment.MiddleLeft;
            InputFormatsButton.UseVisualStyleBackColor = true;
            ToolTip.SetToolTip(InputFormatsButton, "Какие форматы брать из папки импорта");
            InputFormatsButton.Click += InputFormatsButton_Click;
            //
            // QualityLabel
            //
            QualityLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            QualityLabel.AutoSize = true;
            QualityLabel.Location = new Point(355, 108);
            QualityLabel.Name = "QualityLabel";
            QualityLabel.Size = new Size(58, 15);
            QualityLabel.TabIndex = 9;
            QualityLabel.Text = "Качество:";
            //
            // QualityUpDown
            //
            QualityUpDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            QualityUpDown.Location = new Point(415, 105);
            QualityUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            QualityUpDown.Name = "QualityUpDown";
            QualityUpDown.Size = new Size(106, 23);
            QualityUpDown.TabIndex = 10;
            QualityUpDown.Value = new decimal(new int[] { 95, 0, 0, 0 });
            ToolTip.SetToolTip(QualityUpDown, "Качество JPG и WEBP (100 для WEBP — без потерь)");
            //
            // UpscaleCheckBox
            //
            UpscaleCheckBox.Location = new Point(14, 136);
            UpscaleCheckBox.Name = "UpscaleCheckBox";
            UpscaleCheckBox.Size = new Size(185, 20);
            UpscaleCheckBox.TabIndex = 11;
            UpscaleCheckBox.Text = "Увеличивать мелкие DDS до";
            ToolTip.SetToolTip(UpscaleCheckBox, "Увеличивать DDS в целое число раз, пока большая сторона не достигнет размера");
            UpscaleCheckBox.CheckedChanged += Option_Changed;
            //
            // UpscaleSizeComboBox
            //
            UpscaleSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            UpscaleSizeComboBox.Location = new Point(205, 134);
            UpscaleSizeComboBox.Name = "UpscaleSizeComboBox";
            UpscaleSizeComboBox.Size = new Size(75, 23);
            UpscaleSizeComboBox.TabIndex = 12;
            //
            // ThreadsLabel
            //
            ThreadsLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ThreadsLabel.AutoSize = true;
            ThreadsLabel.Location = new Point(355, 137);
            ThreadsLabel.Name = "ThreadsLabel";
            ThreadsLabel.Size = new Size(48, 15);
            ThreadsLabel.TabIndex = 13;
            ThreadsLabel.Text = "Потоки:";
            //
            // ThreadsUpDown
            //
            ThreadsUpDown.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ThreadsUpDown.Location = new Point(415, 134);
            ThreadsUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            ThreadsUpDown.Name = "ThreadsUpDown";
            ThreadsUpDown.Size = new Size(106, 23);
            ThreadsUpDown.TabIndex = 14;
            ThreadsUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
            ToolTip.SetToolTip(ThreadsUpDown, "Сколько файлов конвертировать одновременно. Меньше потоков — меньше расход памяти");
            //
            // DownscaleCheckBox
            //
            DownscaleCheckBox.Location = new Point(14, 165);
            DownscaleCheckBox.Name = "DownscaleCheckBox";
            DownscaleCheckBox.Size = new Size(185, 20);
            DownscaleCheckBox.TabIndex = 15;
            DownscaleCheckBox.Text = "Уменьшать больше чем";
            ToolTip.SetToolTip(DownscaleCheckBox, "Уменьшать в 2, 4, 8... раз, пока большая сторона не станет не больше размера");
            DownscaleCheckBox.CheckedChanged += Option_Changed;
            //
            // DownscaleSizeComboBox
            //
            DownscaleSizeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            DownscaleSizeComboBox.Location = new Point(205, 163);
            DownscaleSizeComboBox.Name = "DownscaleSizeComboBox";
            DownscaleSizeComboBox.Size = new Size(75, 23);
            DownscaleSizeComboBox.TabIndex = 16;
            //
            // MipmapsCheckBox
            //
            MipmapsCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            MipmapsCheckBox.Location = new Point(357, 165);
            MipmapsCheckBox.Name = "MipmapsCheckBox";
            MipmapsCheckBox.Size = new Size(164, 20);
            MipmapsCheckBox.TabIndex = 17;
            MipmapsCheckBox.Text = "Мипмапы (DDS)";
            ToolTip.SetToolTip(MipmapsCheckBox, "Создавать мипмапы при сохранении в DDS");
            //
            // ConvertButton
            //
            ConvertButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ConvertButton.Location = new Point(12, 195);
            ConvertButton.Name = "ConvertButton";
            ConvertButton.Size = new Size(270, 27);
            ConvertButton.TabIndex = 18;
            ConvertButton.Text = "Конвертировать";
            ConvertButton.UseVisualStyleBackColor = true;
            ToolTip.SetToolTip(ConvertButton, "Файлы и папки можно просто перетащить на окно");
            ConvertButton.Click += Convert_Click;
            //
            // RetryButton
            //
            RetryButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            RetryButton.Location = new Point(288, 195);
            RetryButton.Name = "RetryButton";
            RetryButton.Size = new Size(129, 27);
            RetryButton.TabIndex = 19;
            RetryButton.Text = "Повторить ошибки";
            RetryButton.UseVisualStyleBackColor = true;
            ToolTip.SetToolTip(RetryButton, "Конвертировать заново только файлы с ошибками из последнего запуска");
            RetryButton.Click += Retry_Click;
            //
            // OpenLogButton
            //
            OpenLogButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            OpenLogButton.Location = new Point(423, 195);
            OpenLogButton.Name = "OpenLogButton";
            OpenLogButton.Size = new Size(98, 27);
            OpenLogButton.TabIndex = 20;
            OpenLogButton.Text = "Открыть лог";
            OpenLogButton.UseVisualStyleBackColor = true;
            OpenLogButton.Click += OpenLog_Click;
            //
            // Status
            //
            Status.AutoSize = true;
            Status.Location = new Point(12, 229);
            Status.Margin = new Padding(4, 0, 4, 0);
            Status.Name = "Status";
            Status.Size = new Size(39, 15);
            Status.TabIndex = 21;
            Status.Text = "Status";
            //
            // Progress
            //
            Progress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Progress.Location = new Point(12, 263);
            Progress.Margin = new Padding(4, 3, 4, 3);
            Progress.Name = "Progress";
            Progress.Size = new Size(509, 23);
            Progress.TabIndex = 22;
            //
            // ErrorListBox
            //
            ErrorListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ErrorListBox.ContextMenuStrip = ErrorMenu;
            ErrorListBox.HorizontalScrollbar = true;
            ErrorListBox.IntegralHeight = false;
            ErrorListBox.ItemHeight = 15;
            ErrorListBox.Location = new Point(12, 292);
            ErrorListBox.Name = "ErrorListBox";
            ErrorListBox.ScrollAlwaysVisible = true;
            ErrorListBox.Size = new Size(509, 124);
            ErrorListBox.TabIndex = 23;
            ToolTip.SetToolTip(ErrorListBox, "Двойной клик — показать файл в проводнике, правый клик — меню");
            ErrorListBox.DoubleClick += ErrorListBox_DoubleClick;
            ErrorListBox.MouseDown += ErrorListBox_MouseDown;
            //
            // ErrorMenu
            //
            ErrorMenu.Items.AddRange(new ToolStripItem[] { OpenFileMenuItem, ShowInExplorerMenuItem, ErrorMenuSeparator, CopyMenuItem, CopyAllMenuItem });
            ErrorMenu.Name = "ErrorMenu";
            ErrorMenu.Size = new Size(200, 98);
            ErrorMenu.Opening += ErrorMenu_Opening;
            //
            // OpenFileMenuItem
            //
            OpenFileMenuItem.Name = "OpenFileMenuItem";
            OpenFileMenuItem.Size = new Size(199, 22);
            OpenFileMenuItem.Text = "Открыть файл";
            OpenFileMenuItem.Click += OpenFileMenuItem_Click;
            //
            // ShowInExplorerMenuItem
            //
            ShowInExplorerMenuItem.Name = "ShowInExplorerMenuItem";
            ShowInExplorerMenuItem.Size = new Size(199, 22);
            ShowInExplorerMenuItem.Text = "Показать в проводнике";
            ShowInExplorerMenuItem.Click += ShowInExplorerMenuItem_Click;
            //
            // ErrorMenuSeparator
            //
            ErrorMenuSeparator.Name = "ErrorMenuSeparator";
            ErrorMenuSeparator.Size = new Size(196, 6);
            //
            // CopyMenuItem
            //
            CopyMenuItem.Name = "CopyMenuItem";
            CopyMenuItem.Size = new Size(199, 22);
            CopyMenuItem.Text = "Копировать";
            CopyMenuItem.Click += CopyMenuItem_Click;
            //
            // CopyAllMenuItem
            //
            CopyAllMenuItem.Name = "CopyAllMenuItem";
            CopyAllMenuItem.Size = new Size(199, 22);
            CopyAllMenuItem.Text = "Копировать все";
            CopyAllMenuItem.Click += CopyAllMenuItem_Click;
            //
            // InputFormatsMenu
            //
            InputFormatsMenu.Name = "InputFormatsMenu";
            InputFormatsMenu.Size = new Size(61, 4);
            InputFormatsMenu.Closing += InputFormatsMenu_Closing;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(536, 428);
            Controls.Add(ImportBox);
            Controls.Add(ImportButton);
            Controls.Add(ExportBox);
            Controls.Add(ExportButton);
            Controls.Add(OverrideCheckBox);
            Controls.Add(OnlyChangedCheckBox);
            Controls.Add(FormatLabel);
            Controls.Add(FormatComboBox);
            Controls.Add(InputFormatsButton);
            Controls.Add(QualityLabel);
            Controls.Add(QualityUpDown);
            Controls.Add(UpscaleCheckBox);
            Controls.Add(UpscaleSizeComboBox);
            Controls.Add(ThreadsLabel);
            Controls.Add(ThreadsUpDown);
            Controls.Add(DownscaleCheckBox);
            Controls.Add(DownscaleSizeComboBox);
            Controls.Add(MipmapsCheckBox);
            Controls.Add(ConvertButton);
            Controls.Add(RetryButton);
            Controls.Add(OpenLogButton);
            Controls.Add(Status);
            Controls.Add(Progress);
            Controls.Add(ErrorListBox);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            RightToLeft = RightToLeft.No;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Texture Converter";
            ((System.ComponentModel.ISupportInitialize)QualityUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)ThreadsUpDown).EndInit();
            ErrorMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TextBox ImportBox;
        private System.Windows.Forms.Button ImportButton;
        private System.Windows.Forms.TextBox ExportBox;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.CheckBox OverrideCheckBox;
        private System.Windows.Forms.CheckBox OnlyChangedCheckBox;
        private System.Windows.Forms.Label FormatLabel;
        private System.Windows.Forms.ComboBox FormatComboBox;
        private System.Windows.Forms.Button InputFormatsButton;
        private System.Windows.Forms.Label QualityLabel;
        private System.Windows.Forms.NumericUpDown QualityUpDown;
        private System.Windows.Forms.CheckBox UpscaleCheckBox;
        private System.Windows.Forms.ComboBox UpscaleSizeComboBox;
        private System.Windows.Forms.Label ThreadsLabel;
        private System.Windows.Forms.NumericUpDown ThreadsUpDown;
        private System.Windows.Forms.CheckBox DownscaleCheckBox;
        private System.Windows.Forms.ComboBox DownscaleSizeComboBox;
        private System.Windows.Forms.CheckBox MipmapsCheckBox;
        private System.Windows.Forms.Button ConvertButton;
        private System.Windows.Forms.Button RetryButton;
        private System.Windows.Forms.Button OpenLogButton;
        private System.Windows.Forms.Label Status;
        private System.Windows.Forms.ProgressBar Progress;
        private System.Windows.Forms.ListBox ErrorListBox;
        private System.Windows.Forms.ContextMenuStrip ErrorMenu;
        private System.Windows.Forms.ToolStripMenuItem OpenFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShowInExplorerMenuItem;
        private System.Windows.Forms.ToolStripSeparator ErrorMenuSeparator;
        private System.Windows.Forms.ToolStripMenuItem CopyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CopyAllMenuItem;
        private System.Windows.Forms.ContextMenuStrip InputFormatsMenu;
        private System.Windows.Forms.ToolTip ToolTip;
    }
}
