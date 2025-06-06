﻿/*
 * Copyright 2015-2025 yoshikixxxx.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 */
using System;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using Granados;
using Poderosa.Commands;
using Poderosa.ConnectionParam;
using Poderosa.Protocols;
using Poderosa.Sessions;
using Poderosa.Terminal;
using Poderosa.View;


namespace Contrib.ConnectProfile {
    /// <summary>
    /// プロファイルリスト操作クラス
    /// </summary>
    internal class Commands {
        // メンバー変数
        private static bool _connectCancelFlg;

        /// <summary>
        /// プロファイルを新規作成
        /// </summary>
        /// <param name="proflist">プロファイルリスト</param>
        public void NewProfileCommand(ConnectProfileList proflist) {
            ProfileEditForm dlg = new ProfileEditForm(null);
            if (dlg.ShowDialog() == DialogResult.OK) {
                AddProfileCommand(proflist, dlg.ResultProfile);
            }
        }

        /// <summary>
        /// カレントセッションプロファイルを新規作成
        /// </summary>
        /// <param name="ts">ターミナルセッション</param>
        public void NewProfileCurrentSessionCommand(ITerminalSession ts) {
            ISSHLoginParameter ssh = (ISSHLoginParameter)ts.TerminalConnection.Destination.GetAdapter(typeof(ISSHLoginParameter));
            ITelnetParameter telnet = (ITelnetParameter)ts.TerminalConnection.Destination.GetAdapter(typeof(ITelnetParameter));
            ITCPParameter tcp = null;
            ConnectProfileStruct prof = new ConnectProfileStruct();

            // プロトコルチェック
            if (telnet != null) {
                // Telnet
                tcp = (ITCPParameter)ts.TerminalConnection.Destination.GetAdapter(typeof(ITCPParameter));
                prof.Protocol = ConnectionMethod.Telnet;
                prof.HostName = tcp.Destination;
                prof.Port = tcp.Port;
                prof.TelnetNewLine = telnet.TelnetNewLine;
            } else {
                // SSH
                tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));
                prof.HostName = tcp.Destination;
                prof.Port = tcp.Port;
                prof.UserName = ssh.Account;
                prof.Password = ssh.PasswordOrPassphrase;
                prof.KeyFile = ssh.IdentityFileName;
                prof.AutoLogin = true;
                if (ssh.Method.ToString() == "SSH1") { prof.Protocol = ConnectionMethod.SSH1; }
                else if (ssh.Method.ToString() == "SSH2") { prof.Protocol = ConnectionMethod.SSH2; }
                if (ssh.AuthenticationType.ToString() == "Password") { prof.AuthType = AuthType.Password; }
                else if (ssh.AuthenticationType.ToString() == "PublicKey") { prof.AuthType = AuthType.PublicKey; }
                else if (ssh.AuthenticationType.ToString() == "KeyboardInteractive") { prof.AuthType = AuthType.KeyboardInteractive; }
            }

            // その他設定
            prof.CharCode = ts.TerminalSettings.Encoding;
            prof.NewLine = ts.TerminalSettings.TransmitNL;
            prof.TerminalType = ts.TerminalSettings.TerminalType;
            if (ts.TerminalSettings.RenderProfile == null) {
                prof.RenderProfile = ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions.CreateRenderProfile();
                prof.RenderProfile.ESColorSet = new EscapesequenceColorSet();
                prof.RenderProfile.ESColorSet.ResetToDefault();
            } else {
                prof.RenderProfile = ts.TerminalSettings.RenderProfile;
            }
            prof.CommandSendInterval = ConnectProfileStruct.DEFAULT_CMD_SEND_INTERVAL;
            prof.PromptRecvTimeout = ConnectProfileStruct.DEFAULT_PROMPT_RECV_TIMEOUT;
            prof.ProfileItemColor = System.Drawing.Color.Black;

            // ウィンドウ表示/プロファイル追加
            ProfileEditForm dlg = new ProfileEditForm(prof);
            if (dlg.ShowDialog() == DialogResult.OK) {
                AddProfileCommand(ConnectProfilePlugin.Profiles, dlg.ResultProfile);
            }
        }

        /// <summary>
        /// プロファイルを追加
        /// </summary>
        /// <param name="proflist">プロファイルリスト</param>
        /// <param name="prof">プロファイル</param>
        public void AddProfileCommand(ConnectProfileList proflist, ConnectProfileStruct prof) {
            proflist.AddProfile(prof);
        }

        /// <summary>
        /// プロファイルを編集
        /// </summary>
        /// <param name="proflist">プロファイルリスト</param>
        /// <param name="prof">プロファイル</param>
        public void EditProfileCommand(ConnectProfileList proflist, ConnectProfileStruct prof) {
            ProfileEditForm dlg = new ProfileEditForm(prof);
            if (dlg.ShowDialog() == DialogResult.OK) {
                proflist.ReplaceProfile(prof, dlg.ResultProfile);
            }
        }

        /// <summary>
        /// プロファイルをコピーして編集
        /// </summary>
        /// <param name="proflist">プロファイルリスト</param>
        /// <param name="prof">コピー元プロファイル</param>
        public void CopyEditProfileCommand(ConnectProfileList proflist, ConnectProfileStruct prof) {
            ProfileEditForm dlg = new ProfileEditForm(prof);
            if (dlg.ShowDialog() == DialogResult.OK) {
                proflist.AddProfile(dlg.ResultProfile);
            }
        }

        /// <summary>
        /// プロファイルを削除
        /// </summary>
        /// <param name="proflist">プロファイルリスト</param>
        /// <param name="prof">プロファイル</param>
        public void DeleteProfileCommand(ConnectProfileList proflist, ConnectProfileStruct prof) {
            proflist.DeleteProfile(prof);
        }

        /// <summary>
        /// 全プロファイルリストを置換
        /// </summary>
        /// <param name="proflist1">置換前プロファイルリスト</param>
        /// <param name="proflist2">置換後プロファイルリスト</param>
        public void ReplaceAllProfileCommand(ConnectProfileList proflist1, ConnectProfileList proflist2) {
            proflist1.ReplaceAllProfile(proflist2);
        }

        /// <summary>
        /// 接続
        /// </summary>
        /// <param name="prof">プロファイル</param>
        public void ConnectProfile(ConnectProfileStruct prof) {
            _connectCancelFlg = false;

            ConnectProfileTerminal terminal = new ConnectProfileTerminal(prof);
            Thread _connectThread = new Thread((ThreadStart)delegate() {
                terminal.Connect();
            });
            _connectThread.IsBackground = true;
            _connectThread.Start();

            // スレッド終了待機(Joinを使用するとフリーズしてしまう)
            while (true) {
                Thread.Sleep(10);
                if (_connectCancelFlg == true) _connectThread.Abort(); // 接続キャンセル(スレッド終了)
                if (_connectThread.IsAlive != true) break; // スレッド終了後break
                System.Windows.Forms.Application.DoEvents();
            }
        }

        /// <summary>
        /// 接続キャンセル
        /// </summary>
        public void ConnectCancel() {
            _connectCancelFlg = true;
        }

        /// <summary>
        /// CSV変換
        /// </summary>
        /// <param name="prof">プロファイル</param>
        /// <param name="hidepw">パスワードを隠す</param>
        public string ConvertCSV(ConnectProfileStruct prof, bool hidepw) {
            return string.Format(
                ConnectProfileStruct.FMT_CSV,
                prof.HostName,
                prof.Protocol.ToString(),
                prof.Port.ToString(),
                prof.AuthType.ToString(),
                prof.KeyFile,
                prof.UserName,
                (hidepw == true) ? "" : prof.Password,
                prof.AutoLogin.ToString(),
                prof.LoginPrompt,
                prof.PasswordPrompt,
                prof.ExecCommand,
                prof.SUUserName,
                (hidepw == true) ? "" : prof.SUPassword,
                prof.SUType,
                prof.CharCode.ToString(),
                prof.NewLine.ToString(),
                prof.TelnetNewLine.ToString(),
                prof.TerminalType.ToString(),
                Convert.ToString(prof.RenderProfile.ForeColor.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.BackColor.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.ESColorSet[0].Color.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.ESColorSet[1].Color.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.ESColorSet[2].Color.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.ESColorSet[3].Color.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.ESColorSet[4].Color.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.ESColorSet[5].Color.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.ESColorSet[6].Color.ToArgb(), 16),
                Convert.ToString(prof.RenderProfile.ESColorSet[7].Color.ToArgb(), 16),
                prof.RenderProfile.FontName,
                prof.RenderProfile.CJKFontName,
                prof.RenderProfile.FontSize.ToString(),
                prof.RenderProfile.UseClearType.ToString(),
                prof.RenderProfile.EnableBoldStyle.ToString(),
                prof.RenderProfile.ForceBoldStyle.ToString(),
                prof.RenderProfile.BackgroundImageFileName,
                prof.RenderProfile.ImageStyle.ToString(),
                prof.CommandSendInterval.ToString(),
                prof.PromptRecvTimeout.ToString(),
                Convert.ToString(prof.ProfileItemColor.ToArgb(), 16),
                prof.Description
            );
        }

        /// <summary>
        /// CSVヘッダー整合性チェック
        /// </summary>
        /// <param name="header">ヘッダー</param>
        public bool CheckCSVHeader(string header) {
            return (header == CSVHeader) ? true : false;
        }

        /// <summary>
        /// CSVフィールド数チェック
        /// </summary>
        /// <param name="data">CSVデータ</param>
        public bool CheckCSVFieldCount(string data) {
            string[] ary = data.Split(',');
            return (ary.Length == ConnectProfileStruct.CSV_FIELD_CNT) ? true : false;
        }

        /// <summary>
        /// CSVデータ整合性チェック
        /// </summary>
        /// <param name="data">CSVデータ</param>
        public ConnectProfileStruct CheckCSVData(string data) {
            ConnectProfileStruct prof = new ConnectProfileStruct();
            ITerminalEmulatorOptions terminalOptions = ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions;
            string[] ary = data.Split(',');
            int tmp;
            float tmpfloat;

            // ホスト名
            if (ary[0] != "") prof.HostName = ary[0];
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidHostName"), MessageBoxIcon.Error);
                return null;
            }

            // プロトコル(
            if (ary[1].ToLower() == "telnet") prof.Protocol = ConnectionMethod.Telnet;
            else if (ary[1].ToLower() == "ssh1") prof.Protocol = ConnectionMethod.SSH1;
            else if (ary[1].ToLower() == "ssh2") prof.Protocol = ConnectionMethod.SSH2;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidProtocol"), MessageBoxIcon.Error);
                return null;
            }

            // ポート
            if (int.TryParse(ary[2], out tmp) == true) prof.Port = tmp;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidPort"), MessageBoxIcon.Error);
                return null;
            }

            // SSH認証方法(空白=Password)
            if ((ary[3].ToLower() == "password") || (ary[3] == "")) prof.AuthType = AuthType.Password;
            else if (ary[3].ToLower() == "publickey") prof.AuthType = AuthType.PublicKey;
            else if (ary[3].ToLower() == "keyboardinteractive") prof.AuthType = AuthType.KeyboardInteractive;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidAuthType"), MessageBoxIcon.Error);
                return null;
            }

            // 秘密鍵ファイル/ユーザ名/パスワード
            prof.KeyFile = ary[4];
            prof.UserName = ary[5];
            prof.Password = ary[6];

            // 自動ログイン(空白=false)
            if (ary[7].ToLower() == "true") prof.AutoLogin = true;
            else if ((ary[7].ToLower() == "false") || ary[7] == "") prof.AutoLogin = false;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidAutoLogin"), MessageBoxIcon.Error);
                return null;
            }

            // プロンプト/実行コマンド/SU
            prof.LoginPrompt = ary[8];
            prof.PasswordPrompt = ary[9];
            prof.ExecCommand = ary[10];
            prof.SUUserName = ary[11];
            prof.SUPassword = ary[12];

            // SUコマンド
            if (ary[13].ToLower() == "") prof.SUType = "";
            else if (ary[13].ToLower() == ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio1")) prof.SUType = ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio1");
            else if (ary[13].ToLower() == ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio2")) prof.SUType = ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio2");
            else if (ary[13].ToLower() == ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio3")) prof.SUType = ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio3");
            else if (ary[13].ToLower() == ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio4")) prof.SUType = ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio4");
            else if (ary[13].ToLower() == ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio5")) prof.SUType = ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio5");
            else if (ary[13].ToLower() == ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio6")) prof.SUType = ConnectProfilePlugin.Strings.GetString("Form.AddProfile._suTypeRadio6");
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidSUType"), MessageBoxIcon.Error);
                return null;
            }

            // 文字コード(空白=UTF8)
            if (ary[14].ToLower() == "iso8859_1") prof.CharCode = EncodingType.ISO8859_1;
            else if ((ary[14].ToLower() == "utf8") || (ary[14]) == "") prof.CharCode = EncodingType.UTF8;
            else if (ary[14].ToLower() == "euc_jp") prof.CharCode = EncodingType.EUC_JP;
            else if (ary[14].ToLower() == "shift_jis") prof.CharCode = EncodingType.SHIFT_JIS;
            else if (ary[14].ToLower() == "gb2312") prof.CharCode = EncodingType.GB2312;
            else if (ary[14].ToLower() == "big5") prof.CharCode = EncodingType.BIG5;
            else if (ary[14].ToLower() == "euc_cn") prof.CharCode = EncodingType.EUC_CN;
            else if (ary[14].ToLower() == "euc_kr") prof.CharCode = EncodingType.EUC_KR;
            else if (ary[14].ToLower() == "utf8_latin") prof.CharCode = EncodingType.UTF8_Latin;
            else if (ary[14].ToLower() == "oem850") prof.CharCode = EncodingType.OEM850;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidCharCode"), MessageBoxIcon.Error);
                return null;
            }

            // 改行コード(空白=CR)
            if ((ary[15].ToLower() == "cr") || (ary[15] == "")) prof.NewLine = NewLine.CR;
            else if (ary[15].ToLower() == "lf") prof.NewLine = NewLine.LF;
            else if (ary[15].ToLower() == "crlf") prof.NewLine = NewLine.CRLF;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidNewLine"), MessageBoxIcon.Error);
                return null;
            }

            // TelnetNewLine(空白=true)
            if ((ary[16].ToLower() == "true") || (ary[16] == "")) prof.TelnetNewLine = true;
            else if (ary[16].ToLower() == "false") prof.TelnetNewLine = false;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidTelnetNewLine"), MessageBoxIcon.Error);
                return null;
            }

            // ターミナル種類(空白=XTerm256Color)
            if (ary[17].ToLower() == "kterm") prof.TerminalType = TerminalType.KTerm;
            else if (ary[17].ToLower() == "vt100") prof.TerminalType = TerminalType.VT100;
            else if (ary[17].ToLower() == "xterm") prof.TerminalType = TerminalType.XTerm;
            else if ((ary[17].ToLower() == "xterm256color") || (ary[17] == "")) prof.TerminalType = TerminalType.XTerm256Color;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidTerminalType"), MessageBoxIcon.Error);
                return null;
            }

            // 表示オプション(背景/フォント/エスケープシーケンス色)
            prof.RenderProfile = ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions.CreateRenderProfile();
            prof.RenderProfile.ForeColor = Poderosa.ParseUtil.ParseColor(ary[18].ToLower(), terminalOptions.TextColor);
            prof.RenderProfile.BackColor = Poderosa.ParseUtil.ParseColor(ary[19].ToLower(), terminalOptions.BGColor);
            prof.RenderProfile.ESColorSet = new EscapesequenceColorSet();
            prof.RenderProfile.ESColorSet.ResetToDefault();
            prof.RenderProfile.ESColorSet[0] = new ESColor(Poderosa.ParseUtil.ParseColor(ary[20].ToLower(), terminalOptions.EscapeSequenceColorSet[0].Color), false);
            prof.RenderProfile.ESColorSet[1] = new ESColor(Poderosa.ParseUtil.ParseColor(ary[21].ToLower(), terminalOptions.EscapeSequenceColorSet[1].Color), false);
            prof.RenderProfile.ESColorSet[2] = new ESColor(Poderosa.ParseUtil.ParseColor(ary[22].ToLower(), terminalOptions.EscapeSequenceColorSet[2].Color), false);
            prof.RenderProfile.ESColorSet[3] = new ESColor(Poderosa.ParseUtil.ParseColor(ary[23].ToLower(), terminalOptions.EscapeSequenceColorSet[3].Color), false);
            prof.RenderProfile.ESColorSet[4] = new ESColor(Poderosa.ParseUtil.ParseColor(ary[24].ToLower(), terminalOptions.EscapeSequenceColorSet[4].Color), false);
            prof.RenderProfile.ESColorSet[5] = new ESColor(Poderosa.ParseUtil.ParseColor(ary[25].ToLower(), terminalOptions.EscapeSequenceColorSet[5].Color), false);
            prof.RenderProfile.ESColorSet[6] = new ESColor(Poderosa.ParseUtil.ParseColor(ary[26].ToLower(), terminalOptions.EscapeSequenceColorSet[6].Color), false);
            prof.RenderProfile.ESColorSet[7] = new ESColor(Poderosa.ParseUtil.ParseColor(ary[27].ToLower(), terminalOptions.EscapeSequenceColorSet[7].Color), false);

            // 表示オプション(フォント)
            prof.RenderProfile.FontName = (ary[28] != "") ? ary[28] : terminalOptions.Font.Name;
            prof.RenderProfile.CJKFontName = (ary[29] != "") ? ary[29] : terminalOptions.CJKFont.Name;
            if (ary[30] == "") prof.RenderProfile.FontSize = terminalOptions.Font.Size;
            else if (float.TryParse(ary[30], out tmpfloat) == true) prof.RenderProfile.FontSize = tmpfloat;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidFontSize"), MessageBoxIcon.Error);
                return null;
            }
            if (ary[31] == "") prof.RenderProfile.UseClearType = terminalOptions.UseClearType;
            else if (ary[31].ToLower() == "true") prof.RenderProfile.UseClearType = true;
            else if (ary[31].ToLower() == "false") prof.RenderProfile.UseClearType = false;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidClearType"), MessageBoxIcon.Error);
                return null;
            }
            if (ary[32] == "") prof.RenderProfile.EnableBoldStyle = terminalOptions.EnableBoldStyle;
            else if (ary[32].ToLower() == "true") prof.RenderProfile.EnableBoldStyle = true;
            else if (ary[32].ToLower() == "false") prof.RenderProfile.EnableBoldStyle = false;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidBoldStyle"), MessageBoxIcon.Error);
                return null;
            }
            if (ary[33] == "") prof.RenderProfile.ForceBoldStyle = terminalOptions.ForceBoldStyle;
            else if (ary[33].ToLower() == "true") prof.RenderProfile.ForceBoldStyle = true;
            else if (ary[33].ToLower() == "false") prof.RenderProfile.ForceBoldStyle = false;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidForceBoldStyle"), MessageBoxIcon.Error);
                return null;
            }

            // 表示オプション(背景画像, ファイルチェックあり)
            if (ary[34] == "") prof.RenderProfile.BackgroundImageFileName = terminalOptions.BackgroundImageFileName;
            else prof.RenderProfile.BackgroundImageFileName = ary[34];
            if (prof.RenderProfile.BackgroundImageFileName != "") {
                try {
                    Image.FromFile(prof.RenderProfile.BackgroundImageFileName);
                } catch (Exception) {
                    ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidBackgroundImageFileName"), MessageBoxIcon.Error);
                    return null;
                }
            }
            if (ary[35] == "") prof.RenderProfile.ImageStyle = terminalOptions.ImageStyle;
            else if (ary[35].ToLower() == "center") prof.RenderProfile.ImageStyle = ImageStyle.Center;
            else if (ary[35].ToLower() == "topleft") prof.RenderProfile.ImageStyle = ImageStyle.TopLeft;
            else if (ary[35].ToLower() == "topright") prof.RenderProfile.ImageStyle = ImageStyle.TopRight;
            else if (ary[35].ToLower() == "bottomleft") prof.RenderProfile.ImageStyle = ImageStyle.BottomLeft;
            else if (ary[35].ToLower() == "bottomright") prof.RenderProfile.ImageStyle = ImageStyle.BottomRight;
            else if (ary[35].ToLower() == "scaled") prof.RenderProfile.ImageStyle = ImageStyle.Scaled;
            else if (ary[35].ToLower() == "horizontalfit") prof.RenderProfile.ImageStyle = ImageStyle.HorizontalFit;
            else if (ary[35].ToLower() == "verticalfit") prof.RenderProfile.ImageStyle = ImageStyle.VerticalFit;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidImageStyle"), MessageBoxIcon.Error);
                return null;
            }

            // コマンド発行間隔
            if (ary[36] == "") prof.CommandSendInterval = ConnectProfileStruct.DEFAULT_CMD_SEND_INTERVAL;
            else if (int.TryParse(ary[36], out tmp) == true) prof.CommandSendInterval = tmp;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidCommandSendInterval"), MessageBoxIcon.Error);
                return null;
            }

            // プロンプト受信タイムアウト
            if (ary[37] == "") prof.PromptRecvTimeout = ConnectProfileStruct.DEFAULT_PROMPT_RECV_TIMEOUT;
            else if (int.TryParse(ary[37], out tmp) == true) prof.PromptRecvTimeout = tmp;
            else {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.ConnectProfile.CSVImportInvalidPromptRecvTimeout"), MessageBoxIcon.Error);
                return null;
            }

            // 項目色
            prof.ProfileItemColor = Poderosa.ParseUtil.ParseColor(ary[38].ToLower(), Color.Black);

            // 説明
            prof.Description = ary[39];

            return prof;
        }

        /// <summary>
        /// CSVヘッダー
        /// </summary>
        public string CSVHeader {
            get {
                return string.Format(
                    ConnectProfileStruct.FMT_CSV,
                    "HOSTNAME",
                    "PROTOCOL",
                    "PORT",
                    "AUTH_TYPE",
                    "KEY_FILE",
                    "USERNAME",
                    "PASSWORD",
                    "AUTO_LOGIN",
                    "LOGIN_PROMPT",
                    "PASSWORD_PROMPT",
                    "EXEC_COMMAND",
                    "SU_USERNAME",
                    "SU_PASSWORD",
                    "SU_TYPE",
                    "CHAR_CODE",
                    "NEWLINE",
                    "TELNET_NEWLINE",
                    "TERMINAL_TYPE",
                    "TERMINAL_FONTCOLOR",
                    "TERMINAL_BGCOLOR",
                    "TERMINAL_ESCCOLOR0",
                    "TERMINAL_ESCCOLOR1",
                    "TERMINAL_ESCCOLOR2",
                    "TERMINAL_ESCCOLOR3",
                    "TERMINAL_ESCCOLOR4",
                    "TERMINAL_ESCCOLOR5",
                    "TERMINAL_ESCCOLOR6",
                    "TERMINAL_ESCCOLOR7",
                    "TERMINAL_ASCIIFONT",
                    "TERMINAL_CJKFONT",
                    "TERMINAL_FONTSIZE",
                    "TERMINAL_CLEARTYPE",
                    "TERMINAL_BOLD",
                    "TERMINAL_FORCEBOLD",
                    "TERMINAL_BGIMAGE",
                    "TERMINAL_BGIMAGE_POS",
                    "COMMAND_SEND_INTERVAL",
                    "PROMPT_RECV_TIMEOUT",
                    "PROFILE_ITEM_COLOR",
                    "DESCRIPTION"
                );
            }
        }
    }




    /// <summary>
    /// ターミナル接続クラス
    /// </summary>
    internal class ConnectProfileTerminal {
        // メンバー変数
        private ConnectProfileStruct _prof;
        private ITerminalSession _terminalSession;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="prof">プロファイル</param>
        public ConnectProfileTerminal(ConnectProfileStruct prof) {
            _prof = prof;
        }

        /// <summary>
        /// 接続
        /// </summary>
        public void Connect() {
            ITCPParameter tcp = null;

            // プロトコル
            if (_prof.Protocol == ConnectionMethod.Telnet) {
                // Telnet
                tcp = ConnectProfilePlugin.Instance.ProtocolService.CreateDefaultTelnetParameter();
                tcp.Destination = _prof.HostName;
                tcp.Port = _prof.Port;
                ITelnetParameter telnetParameter = null;
                telnetParameter = (ITelnetParameter)tcp.GetAdapter(typeof(ITelnetParameter));
                if (telnetParameter != null) telnetParameter.TelnetNewLine = _prof.TelnetNewLine;
            } else if ((_prof.Protocol == ConnectionMethod.SSH1) || (_prof.Protocol == ConnectionMethod.SSH2)) {
                // SSH
                ISSHLoginParameter ssh = ConnectProfilePlugin.Instance.ProtocolService.CreateDefaultSSHParameter();
                tcp = (ITCPParameter)ssh.GetAdapter(typeof(ITCPParameter));
                tcp.Destination = _prof.HostName;
                tcp.Port = _prof.Port;
                ssh.Method = (_prof.Protocol == ConnectionMethod.SSH1) ? SSHProtocol.SSH1 : SSHProtocol.SSH2;
                ssh.Account = _prof.UserName;
                ssh.AuthenticationType = ConvertAuth(_prof.AuthType);
                ssh.PasswordOrPassphrase = _prof.Password;
                ssh.IdentityFileName = _prof.KeyFile;
                ssh.LetUserInputPassword = (_prof.AutoLogin == true) ? false : true;
            }

            // TerminalSettings(表示プロファイル/改行コード/文字コード)
            ITerminalSettings terminalSettings = ConnectProfilePlugin.Instance.TerminalEmulatorService.CreateDefaultTerminalSettings(_prof.HostName, null);
            terminalSettings.BeginUpdate();
            terminalSettings.RenderProfile = _prof.RenderProfile;
            terminalSettings.TransmitNL = _prof.NewLine;
            terminalSettings.Encoding = _prof.CharCode;
            terminalSettings.TerminalType = _prof.TerminalType;
            terminalSettings.LocalEcho = false;
            terminalSettings.EndUpdate();

            // TerminalParameter
            ITerminalParameter terminalParam = (ITerminalParameter)tcp.GetAdapter(typeof(ITerminalParameter));
            terminalParam.SetTerminalName(terminalSettings.TerminalType.ToTermValue());

            // ターミナルサイズ(これを行わないとPoderosa起動直後のOnReceptionが何故か機能しない, 行わない場合は2回目以降の接続時は正常)
            IViewManager viewManager = CommandTargetUtil.AsWindow(ConnectProfilePlugin.Instance.WindowManager.ActiveWindow).ViewManager;
            IContentReplaceableView contentReplaceableView = (IContentReplaceableView)viewManager.GetCandidateViewForNewDocument().GetAdapter(typeof(IContentReplaceableView));
            TerminalControl terminalControl = (TerminalControl)contentReplaceableView.GetCurrentContent().GetAdapter(typeof(TerminalControl));
            if (terminalControl != null) {
                Size size = terminalControl.CalcTerminalSize(terminalSettings.RenderProfile);
                terminalParam.SetTerminalSize(size.Width, size.Height);
            }

            // 接続(セッションオープン)
            _terminalSession = (ITerminalSession)ConnectProfilePlugin.Instance.WindowManager.ActiveWindow.AsForm().Invoke(new OpenSessionDelegate(InvokeOpenSessionOrNull), terminalParam, terminalSettings);

            // 自動ログイン/SU/実行コマンド
            if (_terminalSession != null) {
                // 受信データオブジェクト作成(ユーザからのキーボード入力が不可)
                ReceptionData pool = new ReceptionData();
                _terminalSession.Terminal.StartModalTerminalTask(pool);

                // Telnet自動ログイン
                if ((_prof.AutoLogin == true) && (_prof.Protocol == ConnectionMethod.Telnet)) {
                    if (TelnetAutoLogin() != true) return;
                }

                // SU
                if ((_prof.AutoLogin == true) && (_prof.SUUserName != "")) {
                    if (SUSwitch() != true) return;
                }

                // 実行コマンド
                if ((_prof.AutoLogin == true) && (_prof.ExecCommand != "")) {
                    if (ExecCommand() != true) return;
                }

                // 受信データオブジェクト定義解除(ユーザからのキーボード入力を許可)
                _terminalSession.Terminal.EndModalTerminalTask();
            }
        }

        /// <summary>
        /// SSH認証オブジェクト変換
        /// </summary>
        /// <param name="t">SSH認証方法</param>
        private AuthenticationType ConvertAuth(AuthType t) {
            return t == AuthType.Password ? AuthenticationType.Password : t == AuthType.PublicKey ? AuthenticationType.PublicKey : AuthenticationType.KeyboardInteractive;
        }

        /// <summary>
        /// Telnet自動ログイン
        /// </summary>
        private bool TelnetAutoLogin() {
            // ユーザ名
            if (_prof.UserName != "#") {
                if (WaitRecv(_prof.LoginPrompt) != true) {
                    ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.Connect.LoginPromptNotFound"), MessageBoxIcon.Warning);
                    _terminalSession.Terminal.EndModalTerminalTask();
                    return false;
                }
                TransmitLn(_prof.UserName);
            }

            // パスワード
            if (WaitRecv(_prof.PasswordPrompt) != true) {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.Connect.PasswordPromptNotFound"), MessageBoxIcon.Warning);
                _terminalSession.Terminal.EndModalTerminalTask();
                return false;
            }
            TransmitLn(_prof.Password);

            return true;
        }

        /// <summary>
        /// SUスイッチ
        /// </summary>
        private bool SUSwitch() {
            // ユーザ名
            if (_prof.SUUserName != "#") {
                TransmitLn(string.Format("{0} {1}", _prof.SUType, _prof.SUUserName));
            } else {
                TransmitLn(string.Format("{0}", _prof.SUType));
            }

            // パスワード
            if (WaitRecv(_prof.PasswordPrompt) != true) {
                ConnectProfilePlugin.MessageBoxInvoke(ConnectProfilePlugin.Strings.GetString("Message.Connect.PasswordPromptNotFound"), MessageBoxIcon.Warning);
                _terminalSession.Terminal.EndModalTerminalTask();
                return false;
            }
            TransmitLn(_prof.SUPassword);

            return true;
        }

        /// <summary>
        /// 実行コマンド
        /// </summary>
        private bool ExecCommand() {
            TransmitLn(_prof.ExecCommand);

            return true;
        }

        /// <summary>
        /// 指定文字列受信待機
        /// </summary>
        /// <param name="searchstr">受信待機対象文字列</param>
        private bool WaitRecv(string searchstr) {
            Thread.Sleep(_prof.CommandSendInterval);
            Regex RegExp = new Regex(searchstr, (RegexOptions.Multiline | RegexOptions.IgnoreCase));
            string data = "";

            while (RegExp.IsMatch(data) == false) {
                Thread.Sleep(10);
                data = ReceiveData(_prof.PromptRecvTimeout);
                if (data == null) break;
            }

            return (data == null) ? false : true;
        }

        /// <summary>
        /// 文字列送信
        /// </summary>
        /// <param name="data">送信文字列</param>
        public void Transmit(string data) {
            Thread.Sleep(_prof.CommandSendInterval);
            _terminalSession.TerminalTransmission.SendString(data.ToCharArray());
        }

        /// <summary>
        /// 文字列送信(改行あり)
        /// </summary>
        /// <param name="data">送信文字列</param>
        public void TransmitLn(string data) {
            Thread.Sleep(_prof.CommandSendInterval);
            if (data.Length > 0) _terminalSession.TerminalTransmission.SendString(data.ToCharArray());
            _terminalSession.TerminalTransmission.SendLineBreak();
        }

        /// <summary>
        /// 受信データ読み取り
        /// </summary>
        public string ReceiveData() {
            return GetCurrentReceptionPool().ReadAll();
        }

        /// <summary>
        /// 受信データ読み取り(タイムアウト設定あり)
        /// </summary>
        /// <param name="timeoutMillisecs">タイムアウト(ms)</param>
        public string ReceiveData(int timeoutMillisecs) {
            return GetCurrentReceptionPool().ReadAll(timeoutMillisecs);
        }

        /// <summary>
        /// GetCurrentReceptionPool
        /// </summary>
        private ReceptionData GetCurrentReceptionPool() {
            return (ReceptionData)_terminalSession.Terminal.CurrentModalTerminalTask.GetAdapter(typeof(ReceptionData));
        }

        /// <summary>
        /// ターミナルセッションDelegate
        /// </summary>
        /// <param name="tp">ターミナルパラメータ</param>
        /// <param name="ts">ターミナルセッティング</param>
        private delegate ITerminalSession OpenSessionDelegate(ITerminalParameter tp, ITerminalSettings ts);
        /// <summary>
        /// ターミナルセッションInvoke
        /// </summary>
        /// <param name="tp">ターミナルパラメータ</param>
        /// <param name="ts">ターミナルセッティング</param>
        private static ITerminalSession InvokeOpenSessionOrNull(ITerminalParameter tp, ITerminalSettings ts) {
            try {
                ITerminalSessionsService ss = ConnectProfilePlugin.Instance.TerminalSessionsService;
                ITerminalSession newsession = ss.TerminalSessionStartCommand.StartTerminalSession(ConnectProfilePlugin.Instance.WindowManager.ActiveWindow, tp, ts);
                return (newsession == null) ? null : newsession;
            } catch (Exception ex) {
                Poderosa.RuntimeUtil.ReportException(ex);
                return null;
            }
        }
    }




    /// <summary>
    /// 受信文字列格納保持クラス
    /// </summary>
    internal class ReceptionData : IModalCharacterTask {
        // メンバー変数
        private IModalTerminalTaskSite _site;
        private StringBuilder _buffer;
        private readonly object _signal = new object();
        private bool _closed;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public void InitializeModelTerminalTask(IModalTerminalTaskSite site, IByteAsyncInputStream default_handler, ITerminalConnection connection) {
            _site = site;
            _buffer = new StringBuilder();
            _closed = false;
        }

        /// <summary>
        /// GetAdapter
        /// </summary>
        public Poderosa.IAdaptable GetAdapter(Type adapter) {
            return ConnectProfilePlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        /// <summary>
        /// OnReception
        /// </summary>
        public void OnReception(ByteDataFragment data) {
        }

        /// <summary>
        /// OnNormalTermination
        /// </summary>
        public void OnNormalTermination() {
            lock (_signal) {
                if (!_closed) _site.Cancel(null);
            }
        }

        /// <summary>
        /// OnAbnormalTermination
        /// </summary>
        public void OnAbnormalTermination(string message) {
            lock (_signal) {
                if (!_closed) _site.Cancel(null);
            }
        }

        /// <summary>
        /// NotifyEndOfPacket
        /// </summary>
        public void NotifyEndOfPacket() {
            lock (_signal) {
                if (!_closed) Monitor.PulseAll(_signal);
            }
        }

        /// <summary>
        /// Close
        /// </summary>
        public void Close() {
            lock (_signal) {
                if (!_closed) {
                    _closed = true;
                    Monitor.PulseAll(_signal);
                    _site.Complete();
                }
            }
        }

        /// <summary>
        /// 受信データ読み取り
        /// </summary>
        public string ReadAll() {
            return ReadAllMain(false, 0);
        }

        /// <summary>
        /// 受信データ読み取り(タイムアウトあり)
        /// </summary>
        /// <param name="timeoutMillisecs">タイムアウト(ms)</param>
        public string ReadAll(int timeoutMillisecs) {
            if (timeoutMillisecs < 0) throw new ArgumentException("Invalid timeout", "timeoutMillisecs");
            return ReadAllMain(true, timeoutMillisecs);
        }

        /// <summary>
        /// 受信データ読み取り(実体)
        /// </summary>
        /// <param name="hasTimeout">タイムアウト設定</param>
        /// <param name="timeoutMillisecs">タイムアウト(ms)</param>
        private string ReadAllMain(bool hasTimeout, int timeoutMillisecs) {
            lock (_signal) {
                if (_buffer.Length == 0) {
                    if (_closed) {
                        if (hasTimeout) return null;
                    } else {
                        if (hasTimeout) {
                            bool signaled = Monitor.Wait(_signal, timeoutMillisecs);
                            if (!signaled && _buffer.Length == 0) return null;
                        } else {
                            Monitor.Wait(_signal);
                        }
                    }
                }
            }

            string r;
            lock (_buffer) {
                r = _buffer.ToString();
                _buffer.Remove(0, _buffer.Length);
            }
            return r;
        }

        /// <summary>
        /// 受信データ格納保持
        /// </summary>
        public void ProcessChar(char ch) {
            if (Char.IsControl(ch) && ch != '\n') return;
            lock (_buffer) {
                _buffer.Append(ch);
            }
        }

        /// <summary>
        /// Caption
        /// </summary>
        public string Caption {
            get { return "CONNECTPROFILE"; }
        }

        /// <summary>
        /// ShowInputInTerminal
        /// </summary>
        public bool ShowInputInTerminal {
            get { return true; }
        }
    }
}
