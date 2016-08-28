﻿/*
 * Copyright 2004,2006 The Poderosa Project.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: EditRenderProfile.cs,v 1.6 2012/03/17 16:11:08 kzmi Exp $
 *
 * modified by yoshikixxxx, for ConnectProfilePlugin.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.UI;
using Poderosa.Util;
using Poderosa.View;

namespace Contrib.ConnectProfile {
    internal class EditRenderProfile : System.Windows.Forms.Form {
        private RenderProfile _profile;

        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private Button _setToDefaultButton;
        private System.Windows.Forms.Label _bgColorLabel;
        private ColorButton _bgColorBox;
        private System.Windows.Forms.Label _textColorLabel;
        private ColorButton _textColorBox;
        private Button _editColorEscapeSequence;
        private System.Windows.Forms.Label _fontLabel;
        private System.Windows.Forms.Label _fontDescription;
        private System.Windows.Forms.Button _fontSelectButton;
        private ClearTypeAwareLabel _fontSample;
        private Label _backgroundImageLabel;
        private TextBox _backgroundImageBox;
        private Button _backgroundImageSelectButton;
        private Label _imageStyleLabel;
        private ComboBox _imageStyleBox;

        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.Container components = null;

        public EditRenderProfile(RenderProfile prof) {
            //
            // Windows フォーム デザイナ サポートに必要です。
            //
            InitializeComponent();
            _imageStyleBox.Items.AddRange(EnumListItem<ImageStyle>.GetListItems());
            this._bgColorLabel.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile._bgColorLabel");
            this._textColorLabel.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile._textColorLabel");
            this._fontSelectButton.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile._fontSelectButton");
            this._fontLabel.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile._fontLabel");
            this._backgroundImageLabel.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile._backgroungImageLabel");
            this._imageStyleLabel.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile._imageStyleLabel");
            this._cancelButton.Text = ConnectProfilePlugin.Strings.GetString("Common.Cancel");
            this._okButton.Text = ConnectProfilePlugin.Strings.GetString("Common.OK");
            this._setToDefaultButton.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile._setToDefaultButton");
            this._fontSample.Text = ConnectProfilePlugin.Strings.GetString("Common.FontSample");
            this._editColorEscapeSequence.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile._editEscapeSequenceColorBox");
            this.Text = ConnectProfilePlugin.Strings.GetString("Form.EditRenderProfile.Text");

            _profile = prof == null ? ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions.CreateRenderProfile() : (RenderProfile)prof.Clone();
            InitUI();
        }

        /// <summary>
        /// 使用されているリソースに後処理を実行します。
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード
        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent() {
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._setToDefaultButton = new Button();
            this._bgColorLabel = new System.Windows.Forms.Label();
            this._bgColorBox = new ColorButton();
            this._textColorLabel = new System.Windows.Forms.Label();
            this._textColorBox = new ColorButton();
            this._editColorEscapeSequence = new Button();
            this._fontLabel = new System.Windows.Forms.Label();
            this._fontDescription = new System.Windows.Forms.Label();
            this._fontSelectButton = new System.Windows.Forms.Button();
            this._fontSample = new Contrib.ConnectProfile.ClearTypeAwareLabel();
            this._backgroundImageLabel = new Label();
            this._backgroundImageBox = new TextBox();
            this._backgroundImageSelectButton = new Button();
            this._imageStyleLabel = new Label();
            this._imageStyleBox = new ComboBox();
            this.SuspendLayout();
            //
            // _bgColorLabel
            //
            this._bgColorLabel.Location = new System.Drawing.Point(16, 16);
            this._bgColorLabel.Name = "_bgColorLabel";
            this._bgColorLabel.Size = new System.Drawing.Size(72, 24);
            this._bgColorLabel.TabIndex = 0;
            this._bgColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _bgColorBox
            //
            this._bgColorBox.Location = new System.Drawing.Point(120, 16);
            this._bgColorBox.Name = "_bgColorBox";
            this._bgColorBox.Size = new System.Drawing.Size(112, 20);
            this._bgColorBox.TabIndex = 1;
            _bgColorBox.ColorChanged += new ColorButton.NewColorEventHandler(this.OnBGColorChanged);
            //
            // _textColorLabel
            //
            this._textColorLabel.Location = new System.Drawing.Point(16, 40);
            this._textColorLabel.Name = "_textColorLabel";
            this._textColorLabel.Size = new System.Drawing.Size(72, 24);
            this._textColorLabel.TabIndex = 2;
            this._textColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _textColorBox
            //
            this._textColorBox.Location = new System.Drawing.Point(120, 40);
            this._textColorBox.Name = "_textColorBox";
            this._textColorBox.Size = new System.Drawing.Size(112, 20);
            this._textColorBox.TabIndex = 3;
            this._textColorBox.ColorChanged += new ColorButton.NewColorEventHandler(this.OnTextColorChanged);
            //
            // _editColorEscapeSequence
            //
            this._editColorEscapeSequence.Location = new Point(120, 72);
            this._editColorEscapeSequence.Size = new Size(216, 24);
            this._editColorEscapeSequence.TabIndex = 4;
            this._editColorEscapeSequence.Click += new EventHandler(OnEditColorEscapeSequence);
            this._editColorEscapeSequence.FlatStyle = FlatStyle.System;
            //
            // _fontLabel
            //
            this._fontLabel.Location = new System.Drawing.Point(16, 96);
            this._fontLabel.Name = "_fontLabel";
            this._fontLabel.Size = new System.Drawing.Size(72, 16);
            this._fontLabel.TabIndex = 5;
            this._fontLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _fontDescription
            //
            this._fontDescription.Location = new System.Drawing.Point(120, 100);
            this._fontDescription.Name = "_fontDescription";
            this._fontDescription.Size = new System.Drawing.Size(168, 24);
            this._fontDescription.TabIndex = 6;
            this._fontDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _fontSelectButton
            //
            this._fontSelectButton.Location = new System.Drawing.Point(296, 100);
            this._fontSelectButton.Name = "_fontSelectButton";
            this._fontSelectButton.FlatStyle = FlatStyle.System;
            this._fontSelectButton.Size = new System.Drawing.Size(64, 23);
            this._fontSelectButton.TabIndex = 7;
            this._fontSelectButton.Click += new System.EventHandler(this.OnFontSelect);
            //
            // _fontSample
            //
            this._fontSample.BackColor = System.Drawing.Color.White;
            this._fontSample.ClearType = false;
            this._fontSample.Location = new System.Drawing.Point(240, 16);
            this._fontSample.Name = "_fontSample";
            this._fontSample.Size = new System.Drawing.Size(120, 46);
            this._fontSample.TabIndex = 8;
            this._fontSample.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // _backgroundImageLabel
            //
            this._backgroundImageLabel.Location = new System.Drawing.Point(16, 128);
            this._backgroundImageLabel.Name = "_backgroundImageLabel";
            this._backgroundImageLabel.Size = new System.Drawing.Size(72, 16);
            this._backgroundImageLabel.TabIndex = 9;
            this._backgroundImageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _backgroundImageBox
            //
            this._backgroundImageBox.Location = new System.Drawing.Point(120, 128);
            this._backgroundImageBox.Name = "_backgroundImageBox";
            this._backgroundImageBox.Size = new System.Drawing.Size(220, 19);
            this._backgroundImageBox.TabIndex = 10;
            //
            // _backgroundImageSelectButton
            //
            this._backgroundImageSelectButton.Location = new System.Drawing.Point(340, 128);
            this._backgroundImageSelectButton.Name = "_backgroundImageSelectButton";
            this._backgroundImageSelectButton.FlatStyle = FlatStyle.System;
            this._backgroundImageSelectButton.Size = new System.Drawing.Size(19, 19);
            this._backgroundImageSelectButton.TabIndex = 11;
            this._backgroundImageSelectButton.Text = "...";
            this._backgroundImageSelectButton.Click += new EventHandler(OnSelectBackgroundImage);
            //
            // _imageStyleLabel
            //
            this._imageStyleLabel.Location = new System.Drawing.Point(16, 152);
            this._imageStyleLabel.Name = "_imageStyleLabel";
            this._imageStyleLabel.Size = new System.Drawing.Size(96, 16);
            this._imageStyleLabel.TabIndex = 12;
            this._imageStyleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _imageStyleBox
            //
            this._imageStyleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._imageStyleBox.Location = new System.Drawing.Point(120, 152);
            this._imageStyleBox.Name = "_imageStyleBox";
            this._imageStyleBox.Size = new System.Drawing.Size(112, 19);
            this._imageStyleBox.TabIndex = 13;
            //
            // _okButton
            //
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(208, 180);
            this._okButton.Name = "_okButton";
            this._okButton.FlatStyle = FlatStyle.System;
            this._okButton.TabIndex = 14;
            this._okButton.Click += new EventHandler(OnOK);
            //
            // _cancelButton
            //
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(288, 180);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.FlatStyle = FlatStyle.System;
            this._cancelButton.TabIndex = 15;
            //
            // _setToDefaultButton
            //
            this._setToDefaultButton.Location = new System.Drawing.Point(16, 180);
            this._setToDefaultButton.Size = new Size(96, 23);
            this._setToDefaultButton.Name = "_setToDefaultButton";
            this._setToDefaultButton.FlatStyle = FlatStyle.System;
            this._setToDefaultButton.TabIndex = 16;
            this._setToDefaultButton.Click += new EventHandler(OnSetToDefault);
            //
            // EditRenderProfile
            //
            this.AcceptButton = this._okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(368, 208);
            this.Controls.Add(this._setToDefaultButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._bgColorLabel);
            this.Controls.Add(this._bgColorBox);
            this.Controls.Add(this._textColorLabel);
            this.Controls.Add(this._textColorBox);
            this.Controls.Add(this._editColorEscapeSequence);
            this.Controls.Add(this._fontLabel);
            this.Controls.Add(this._fontDescription);
            this.Controls.Add(this._fontSelectButton);
            this.Controls.Add(this._fontSample);
            this.Controls.Add(this._backgroundImageLabel);
            this.Controls.Add(this._backgroundImageBox);
            this.Controls.Add(this._backgroundImageSelectButton);
            this.Controls.Add(this._imageStyleLabel);
            this.Controls.Add(this._imageStyleBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditRenderProfile";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ResumeLayout(false);

        }
        #endregion
        private void OnBGColorChanged(object sender, Color e) {
            _fontSample.BackColor = e;
            _profile.BackColor = e;
        }
        private void OnTextColorChanged(object sender, Color e) {
            _fontSample.ForeColor = e;
            _profile.ForeColor = e;
        }
        private void OnEditColorEscapeSequence(object sender, EventArgs args) {
            EditEscapeSequenceColor dlg = new EditEscapeSequenceColor(_fontSample.BackColor, _fontSample.ForeColor, _profile.ESColorSet);
            if (dlg.ShowDialog(this) == DialogResult.OK) {
                _profile.ESColorSet = dlg.Result;
                _profile.ForeColor = dlg.GForeColor;
                _profile.BackColor = dlg.GBackColor;
                _bgColorBox.SelectedColor = dlg.GBackColor;
                _textColorBox.SelectedColor = dlg.GForeColor;
                _fontSample.ForeColor = dlg.GForeColor;
                _fontSample.BackColor = dlg.GBackColor;
                _fontSample.Invalidate();
                _bgColorBox.Invalidate();
                _textColorBox.Invalidate();
            }
        }

        private void OnFontSelect(object sender, EventArgs args) {
            GFontDialog gd = new GFontDialog();
            Font nf = Poderosa.RuntimeUtil.CreateFont(_profile.FontName, _profile.FontSize);
            Font jf = Poderosa.RuntimeUtil.CreateFont(_profile.CJKFontName, _profile.FontSize);
            gd.SetFont(_profile.UseClearType, _profile.EnableBoldStyle, _profile.ForceBoldStyle, nf, jf);
            if (gd.ShowDialog(this) == DialogResult.OK) {
                Font f = gd.ASCIIFont;
                _profile.FontName = f.Name;
                _profile.CJKFontName = gd.CJKFont.Name;
                _profile.FontSize = f.Size;
                _profile.UseClearType = gd.UseClearType;
                _profile.EnableBoldStyle = gd.EnableBoldStyle;
                _fontSample.Font = f;
                AdjustFontDescription(f.Name, gd.CJKFont.Name, f.Size);
            }
        }
        private void OnSelectBackgroundImage(object sender, EventArgs args) {
            string t = SelectPictureFileByDialog(this);
            if (t != null) {
                _backgroundImageBox.Text = t;
                _profile.BackgroundImageFileName = t;
            }
        }
        private void AdjustFontDescription(string ascii, string cjk, float fsz) {
            int sz = (int)(fsz + 0.5); //Singleをintにキャストすると切り捨てだが、四捨五入にしてほしいので0.5を足してから切り捨てる
            if (ascii == cjk)
                _fontDescription.Text = String.Format("{0},{1}pt", ascii, sz);
            else
                _fontDescription.Text = String.Format("{0}/{1},{2}pt", ascii, cjk, sz);
        }

        private void InitUI() {
            AdjustFontDescription(_profile.FontName, _profile.CJKFontName, _profile.FontSize);
            _fontSample.Font = Poderosa.RuntimeUtil.CreateFont(_profile.FontName, _profile.FontSize);
            _fontSample.BackColor = _profile.BackColor;
            _fontSample.ForeColor = _profile.ForeColor;
            _fontSample.ClearType = _profile.UseClearType;
            _fontSample.Invalidate(true);
            _backgroundImageBox.Text = _profile.BackgroundImageFileName;
            _bgColorBox.SelectedColor = _profile.BackColor;
            _textColorBox.SelectedColor = _profile.ForeColor;
            _imageStyleBox.SelectedItem = _profile.ImageStyle;  // select EnumListItem<T> by T
        }

        private void OnOK(object sender, EventArgs args) {
            if (_backgroundImageBox.Text.Length > 0) {
                try {
                    Image.FromFile(_backgroundImageBox.Text);
                }
                catch (Exception) {
                    ConnectProfilePlugin.MessageBoxInvoke(string.Format(ConnectProfilePlugin.Strings.GetString("Message.EditRenderProfile.InvalidPicture"), _backgroundImageBox.Text), MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }
            }
            _profile.BackgroundImageFileName = _backgroundImageBox.Text;
            _profile.ImageStyle = ((EnumListItem<ImageStyle>)_imageStyleBox.SelectedItem).Value;
        }

        private void OnSetToDefault(object sender, EventArgs args) {
            _profile = ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions.CreateRenderProfile();
            InitUI();
        }

        public RenderProfile Result {
            get {
                return _profile;
            }
        }


        public static string SelectPictureFileByDialog(Form parent) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            //dlg.InitialDirectory = GApp.Options.DefaultFileDir;
            dlg.Title = ConnectProfilePlugin.Strings.GetString("Util.SelectPicture.Caption");
            dlg.Filter = "Picture Files(*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            if (dlg.ShowDialog(parent) == DialogResult.OK) {
                //GApp.Options.DefaultFileDir = FileToDir(dlg.FileName);
                return dlg.FileName;
            }
            else
                return null;
        }
    }
}
