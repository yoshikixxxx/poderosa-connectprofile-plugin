/*
 * Copyright 2015-2025 yoshikixxxx.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 */
using System;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.Commands;
using Poderosa.ConnectionParam;
using Poderosa.Forms;
using Poderosa.Plugins;
using Poderosa.Preferences;
using Poderosa.Protocols;
using Poderosa.Sessions;
using Poderosa.Terminal;
using Poderosa.View;


/********* アセンブリ情報 *********/
[assembly: PluginDeclaration(typeof(Contrib.ConnectProfile.ConnectProfilePlugin))]
/**********************************/


namespace Contrib.ConnectProfile {
    /********* プラグイン情報 *********/
    [PluginInfo(
        ID = PLUGIN_ID,
        Version = "2.2",
        Author = "yoshikixxxx",
        Dependencies = "org.poderosa.core.commands;org.poderosa.core.preferences;org.poderosa.core.serializing;org.poderosa.core.sessions;org.poderosa.core.window;org.poderosa.protocols;org.poderosa.telnet_ssh;org.poderosa.terminalemulator;org.poderosa.terminalsessions;org.poderosa.terminalui;org.poderosa.usability"
    )]
    /**********************************/




    /// <summary>
    /// ConnectProfileプラグインメインクラス
    /// </summary>
    internal class ConnectProfilePlugin : PluginBase {
        // メンバー変数
        public const string PLUGIN_ID = "contrib.yoshikixxxx.connectprofile";
        public const string CMD_ID_MAIN = PLUGIN_ID + ".main";
        public const string CMD_ID_ADDPROFILE = PLUGIN_ID + ".addprofile";
        private static ConnectProfilePlugin _instance;
        private static ICoreServices _coreServices;
        private static Poderosa.StringResource _stringResource;
        private static ConnectProfileList _profiles;
        private static ConnectProfileOptionsSupplier _connectProfileOptionSupplier;
        private ITerminalEmulatorService _terminalEmulatorPlugin;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public override void InitializePlugin(IPoderosaWorld poderosa) {
            base.InitializePlugin(poderosa);
            _instance = this;

            // 文字列リソース読み込み
            _stringResource = new Poderosa.StringResource("Contrib.ConnectProfile.strings", typeof(ConnectProfilePlugin).Assembly);
            ConnectProfilePlugin.Instance.PoderosaWorld.Culture.AddChangeListener(_stringResource);

            // コマンド登録
            IPluginManager pm = poderosa.PluginManager;
            _coreServices = (ICoreServices)poderosa.GetAdapter(typeof(ICoreServices));
            _terminalEmulatorPlugin = (ITerminalEmulatorService)pm.FindPlugin("org.poderosa.terminalemulator", typeof(ITerminalEmulatorService));
            ConnectProfileCommand.Register(_coreServices.CommandManager);

            // メニューリスト作成
            ConnectProfileMenuGroup menulist = new ConnectProfileMenuGroup();
            ConnectProfileContextMenuGroup contextmenulist = new ConnectProfileContextMenuGroup();

            // メニュー登録
            IExtensionPoint toolmenu = pm.FindExtensionPoint("org.poderosa.menu.tool");
            toolmenu.RegisterExtension(menulist);

            // コンテキストメニュー登録
            IExtensionPoint contextmenu = pm.FindExtensionPoint("org.poderosa.terminalemulator.contextMenu");
            contextmenu.RegisterExtension(contextmenulist);

            // 設定ファイル連携
            _connectProfileOptionSupplier = new ConnectProfileOptionsSupplier();
            _coreServices.PreferenceExtensionPoint.RegisterExtension(_connectProfileOptionSupplier);

            // 接続プロファイル
            _profiles = new ConnectProfileList();
        }

        /// <summary>
        /// プラグイン終了
        /// </summary>
        public override void TerminatePlugin() {
            base.TerminatePlugin();
            _connectProfileOptionSupplier.SaveToPreference();
        }

        /// <summary>
        /// メッセージ表示Delegate
        /// </summary>
        private delegate void MessageBoxDelegate(IWin32Window window, string msg, MessageBoxIcon icon);
        /// <summary>
        /// メッセージ表示Invoke
        /// </summary>
        /// <param name="msg">メッセージ文字列</param>
        /// <param name="icon">アイコン</param>
        public static void MessageBoxInvoke(string msg, MessageBoxIcon icon) {
            Form f = ConnectProfilePlugin.Instance.WindowManager.ActiveWindow.AsForm();
            f.Invoke(new MessageBoxDelegate(Poderosa.GUtil.Warning), f, msg, icon);
        }

        /// <summary>
        /// 選択メッセージ表示Delegate
        /// </summary>
        private delegate DialogResult AskUserYesNoDelegate(IWin32Window window, string msg, MessageBoxIcon icon);
        /// <summary>
        /// 選択メッセージ表示Invoke
        /// </summary>
        /// <param name="msg">メッセージ文字列</param>
        /// <param name="icon">アイコン</param>
        public static DialogResult AskUserYesNoInvoke(string msg, MessageBoxIcon icon) {
            Form f = ConnectProfilePlugin.Instance.WindowManager.ActiveWindow.AsForm();
            return (DialogResult)f.Invoke(new AskUserYesNoDelegate(Poderosa.GUtil.AskUserYesNo), f, msg, icon);
        }

        /// <summary>
        /// インスタンス
        /// </summary>
        public static ConnectProfilePlugin Instance {
            get { return _instance; }
        }

        /// <summary>
        /// 文字列リソース
        /// </summary>
        public static Poderosa.StringResource Strings {
            get { return _stringResource; }
        }

        /// <summary>
        /// プロファイルリスト
        /// </summary>
        public static ConnectProfileList Profiles {
            get { return _profiles; }
        }

        /// <summary>
        /// 設定ファイル
        /// </summary>
        public ConnectProfileOptionsSupplier ConnectProfileOptionSupplier {
            get { return _connectProfileOptionSupplier; }
        }

        /// <summary>
        /// CommandManager
        /// </summary>
        public ICommandManager CommandManager {
            get { return _coreServices.CommandManager; }
        }

        /// <summary>
        /// WindowManager
        /// </summary>
        public IWindowManager WindowManager {
            get { return _coreServices.WindowManager; }
        }

        /// <summary>
        /// TerminalEmulatorService
        /// </summary>
        public ITerminalEmulatorService TerminalEmulatorService {
            get { return (ITerminalEmulatorService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.terminalemulator", typeof(ITerminalEmulatorService)); }
        }

        /// <summary>
        /// TerminalSessionsService
        /// </summary>
        public ITerminalSessionsService TerminalSessionsService {
            get { return (ITerminalSessionsService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.terminalsessions", typeof(ITerminalSessionsService)); }
        }

        /// <summary>
        /// ProtocolService
        /// </summary>
        public IProtocolService ProtocolService {
            get { return (IProtocolService)_poderosaWorld.PluginManager.FindPlugin("org.poderosa.protocols", typeof(IProtocolService)); }
        }
    }




    /// <summary>
    /// プラグイン実行クラス
    /// </summary>
    internal class ConnectProfileCommand : GeneralCommandImpl {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="cmdID">コマンドID</param>
        /// <param name="textID">コマンド説明文ID</param>
        /// <param name="body">実行Delegate</param>
        public ConnectProfileCommand(string cmdID, string textID, ExecuteDelegate body)
            :
            base(cmdID, ConnectProfilePlugin.Strings, textID, ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalCommandCategory, body) {
        }

        /// <summary>
        /// コンストラクタ(実行可否チェックあり)
        /// </summary>
        /// <param name="cmdID">コマンドID</param>
        /// <param name="textID">コマンド説明文ID</param>
        /// <param name="body">実行Delegate</param>
        /// <param name="enabled">実行可否Delegate</param>
        public ConnectProfileCommand(string cmdID, string textID, ExecuteDelegate body, CanExecuteDelegate enabled)
            :
            base(cmdID, ConnectProfilePlugin.Strings, textID, ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalCommandCategory, body, enabled) {
        }

        /// <summary>
        /// コマンド登録
        /// </summary>
        /// <param name="cm">コマンドマネージャ</param>
        public static void Register(ICommandManager cm) {
            // 実行可否Delegate
            CanExecuteDelegate does_open_target_session = new CanExecuteDelegate(DoesOpenTargetSession);

            // 登録
            cm.Register(new ConnectProfileCommand(ConnectProfilePlugin.CMD_ID_MAIN, "Command.ConnectProfile", new ExecuteDelegate(MainWindow)));
            cm.Register(new ConnectProfileCommand(ConnectProfilePlugin.CMD_ID_ADDPROFILE, "Command.AddConnectProfile", new ExecuteDelegate(AddConnectProfile), does_open_target_session));
        }

        /// <summary>
        /// 実行可否チェックDelegate
        /// </summary>
        public static EnabledDelegate DoesOpenTargetSession {
            get {
                return delegate(ICommandTarget target) {
                    ITerminalSession s = AsTerminalSession(target);
                    return (s != null) && (!s.TerminalTransmission.Connection.IsClosed);
                };
            }
        }

        /// <summary>
        /// CommandTargetからTerminalSessionを取得
        /// </summary>
        /// <param name="target">CommandTarget</param>
        public static ITerminalSession AsTerminalSession(ICommandTarget target) {
            IPoderosaDocument doc = CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target);
            if (doc != null) {
                ISession s = doc.OwnerSession;
                return (ITerminalSession)s.GetAdapter(typeof(ITerminalSession));
            } else {
                return null;
            }
        }

        /// <summary>
        /// メインウィンドウ表示コマンド
        /// </summary>
        /// <param name="target">CommandTarget</param>
        private static CommandResult MainWindow(ICommandTarget target) {
            IPoderosaMainWindow window = CommandTargetUtil.AsWindow(target);
            if (window != null) {
                ConnectProfileForm Form = new ConnectProfileForm();
                if (Form.ShowDialog(window.AsForm()) == DialogResult.OK) {
                    return CommandResult.Succeeded;
                }
            }
            return CommandResult.Cancelled;
        }

        /// <summary>
        /// アクティブセッション追加コマンド
        /// </summary>
        /// <param name="target">CommandTarget</param>
        private static CommandResult AddConnectProfile(ICommandTarget target) {
            IPoderosaDocument document = CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target);
            if (document != null) {
                ITerminalSession ts = (ITerminalSession)document.OwnerSession.GetAdapter(typeof(ITerminalSession));
                if (ts != null) {
                    Commands _cmd = new Commands();
                    _cmd.NewProfileCurrentSessionCommand(ts);
                    return CommandResult.Succeeded;
                }
            }
            return CommandResult.Cancelled;
        }
    }




    /// <summary>
    /// 標準メニューアイテムクラス
    /// </summary>
    internal class StandardMenuItem : PoderosaMenuItemImpl {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="commandID">コマンドID</param>
        /// <param name="textID">メニュ文字列ID</param>
        public StandardMenuItem(string commandID, string textID)
            : base(commandID, ConnectProfilePlugin.Strings, textID) {
        }

        /// <summary>
        /// コンストラクタ(チェック付きメニュー)
        /// </summary>
        /// <param name="commandID">コマンドID</param>
        /// <param name="textID">メニュ文字列ID</param>
        /// <param name="cd">チェック付きメニューDelegate</param>
        public StandardMenuItem(string commandID, string textID, CheckedDelegate cd)
            : base(commandID, ConnectProfilePlugin.Strings, textID) {
            _checked = cd;
        }
    }




    /// <summary>
    /// 標準メニューグループクラス
    /// </summary>
    internal abstract class StandarMenuGroup : PoderosaMenuGroupImpl {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public StandarMenuGroup() {
            _designationTarget = null;
            _positionType = PositionType.First;
        }
    }




    /// <summary>
    /// メニュークラス
    /// </summary>
    internal class ConnectProfileMenuGroup : StandarMenuGroup {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public override IPoderosaMenu[] ChildMenus {
            get {
                return new IPoderosaMenu[] {
                    new StandardMenuItem(ConnectProfilePlugin.CMD_ID_MAIN, "Menu.ConnectProfile"),
                    new StandardMenuItem(ConnectProfilePlugin.CMD_ID_ADDPROFILE, "Menu.AddConnectProfile")
                };
            }
        }

        public override PositionType DesignationPosition {
            get {
                return PositionType.Last;
            }
        }
    }




    /// <summary>
    /// コンテキストメニュークラス
    /// </summary>
    internal class ConnectProfileContextMenuGroup : StandarMenuGroup {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public override IPoderosaMenu[] ChildMenus {
            get {
                return new IPoderosaMenu[] {
                    new StandardMenuItem(ConnectProfilePlugin.CMD_ID_ADDPROFILE, "Menu.AddConnectProfile")
                };
            }
        }

        public override PositionType DesignationPosition {
            get {
                return PositionType.Last;
            }
        }
    }




    /// <summary>
    /// 設定ファイル保存/読み込みクラス
    /// </summary>
    internal class ConnectProfileOptionsSupplier : IPreferenceSupplier {
        // メンバー変数
        private IPreferenceFolder _rootPreference;
        private IPreferenceFolder _profileDefinition;
        private IStringPreferenceItem _hostName;
        private IStringPreferenceItem _protocol;
        private IIntPreferenceItem _port;
        private IStringPreferenceItem _authType;
        private IStringPreferenceItem _keyFile;
        private IStringPreferenceItem _userName;
        private IStringPreferenceItem _password;
        private IBoolPreferenceItem _autoLogin;
        private IStringPreferenceItem _loginPrompt;
        private IStringPreferenceItem _passwordPrompt;
        private IStringPreferenceItem _execCommand;
        private IStringPreferenceItem _suUserName;
        private IStringPreferenceItem _suPassword;
        private IStringPreferenceItem _suType;
        private IStringPreferenceItem _charCode;
        private IStringPreferenceItem _newLine;
        private IBoolPreferenceItem _telnetNewLine;
        private IStringPreferenceItem _terminalType;
        private ColorPreferenceItem _terminalFontColor;
        private ColorPreferenceItem _terminalBGColor;
        private ColorPreferenceItem _terminalESCColor0;
        private ColorPreferenceItem _terminalESCColor1;
        private ColorPreferenceItem _terminalESCColor2;
        private ColorPreferenceItem _terminalESCColor3;
        private ColorPreferenceItem _terminalESCColor4;
        private ColorPreferenceItem _terminalESCColor5;
        private ColorPreferenceItem _terminalESCColor6;
        private ColorPreferenceItem _terminalESCColor7;
        private IStringPreferenceItem _terminalAsciiFont;
        private IStringPreferenceItem _terminalCjkFont;
        private IIntPreferenceItem _terminalFontSize;
        private IBoolPreferenceItem _terminalClearType;
        private IBoolPreferenceItem _terminalBoldStyle;
        private IBoolPreferenceItem _terminalForceBoldStyle;
        private IStringPreferenceItem _terminalBGImage;
        private IStringPreferenceItem _terminalBGImagePos;
        private IIntPreferenceItem _commandSendInterval;
        private IIntPreferenceItem _promptRecvTimeout;
        private ColorPreferenceItem _profileItemColor;
        private IStringPreferenceItem _description;
        private bool _preferenceLoaded = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public void InitializePreference(IPreferenceBuilder builder, IPreferenceFolder folder) {
            ITerminalEmulatorOptions terminalOptions = ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions;

            _rootPreference = folder;
            _profileDefinition = builder.DefineFolderArray(folder, this, "profile");
            _hostName = builder.DefineStringValue(_profileDefinition, "hostName", "", null);
            _protocol = builder.DefineStringValue(_profileDefinition, "protocol", "", null);
            _port = builder.DefineIntValue(_profileDefinition, "port", 0, null);
            _authType = builder.DefineStringValue(_profileDefinition, "authType", "", null);
            _keyFile = builder.DefineStringValue(_profileDefinition, "keyFile", "", null);
            _userName = builder.DefineStringValue(_profileDefinition, "userName", "", null);
            _password = builder.DefineStringValue(_profileDefinition, "password", "", null);
            _autoLogin = builder.DefineBoolValue(_profileDefinition, "autoLogin", false, null);
            _loginPrompt = builder.DefineStringValue(_profileDefinition, "loginPrompt", "", null);
            _passwordPrompt = builder.DefineStringValue(_profileDefinition, "passwordPrompt", "", null);
            _execCommand = builder.DefineStringValue(_profileDefinition, "execCommand", "", null);
            _suUserName = builder.DefineStringValue(_profileDefinition, "suUserName", "", null);
            _suPassword = builder.DefineStringValue(_profileDefinition, "suPassword", "", null);
            _suType = builder.DefineStringValue(_profileDefinition, "suType", "", null);
            _charCode = builder.DefineStringValue(_profileDefinition, "charCode", "", null);
            _newLine = builder.DefineStringValue(_profileDefinition, "newLine", "", null);
            _telnetNewLine = builder.DefineBoolValue(_profileDefinition, "telnetNewLine", true, null);
            _terminalType = builder.DefineStringValue(_profileDefinition, "terminalType", "", null);
            _terminalFontColor = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalFontColor", terminalOptions.TextColor.Name, null), KnownColor.White);
            _terminalBGColor = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalBGColor", terminalOptions.BGColor.Name, null), KnownColor.Black);
            _terminalESCColor0 = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalESCColor0", terminalOptions.EscapeSequenceColorSet[0].Color.Name, null), KnownColor.Black);
            _terminalESCColor1 = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalESCColor1", terminalOptions.EscapeSequenceColorSet[1].Color.Name, null), KnownColor.Red);
            _terminalESCColor2 = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalESCColor2", terminalOptions.EscapeSequenceColorSet[2].Color.Name, null), KnownColor.Green);
            _terminalESCColor3 = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalESCColor3", terminalOptions.EscapeSequenceColorSet[3].Color.Name, null), KnownColor.Yellow);
            _terminalESCColor4 = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalESCColor4", terminalOptions.EscapeSequenceColorSet[4].Color.Name, null), KnownColor.Blue);
            _terminalESCColor5 = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalESCColor5", terminalOptions.EscapeSequenceColorSet[5].Color.Name, null), KnownColor.Magenta);
            _terminalESCColor6 = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalESCColor6", terminalOptions.EscapeSequenceColorSet[6].Color.Name, null), KnownColor.Cyan);
            _terminalESCColor7 = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "terminalESCColor7", terminalOptions.EscapeSequenceColorSet[7].Color.Name, null), KnownColor.White);
            _terminalAsciiFont = builder.DefineStringValue(_profileDefinition, "terminalAsciiFont", terminalOptions.Font.Name, null);
            _terminalCjkFont = builder.DefineStringValue(_profileDefinition, "terminalCjkFont", terminalOptions.CJKFont.Name, null);
            _terminalFontSize = builder.DefineIntValue(_profileDefinition, "terminalFontSize", (int)terminalOptions.Font.Size, null);
            _terminalClearType = builder.DefineBoolValue(_profileDefinition, "terminalClearType", terminalOptions.UseClearType, null);
            _terminalBoldStyle = builder.DefineBoolValue(_profileDefinition, "terminalBoldStyle", terminalOptions.EnableBoldStyle, null);
            _terminalForceBoldStyle = builder.DefineBoolValue(_profileDefinition, "terminalForceBoldStyle", terminalOptions.ForceBoldStyle, null);
            _terminalBGImage = builder.DefineStringValue(_profileDefinition, "terminalBGImage", terminalOptions.BackgroundImageFileName, null);
            _terminalBGImagePos = builder.DefineStringValue(_profileDefinition, "terminalBGImagePos", terminalOptions.ImageStyle.ToString(), null);
            _commandSendInterval = builder.DefineIntValue(_profileDefinition, "commandSendInterval", ConnectProfileStruct.DEFAULT_CMD_SEND_INTERVAL, null);
            _promptRecvTimeout = builder.DefineIntValue(_profileDefinition, "promptRecvTimeout", ConnectProfileStruct.DEFAULT_PROMPT_RECV_TIMEOUT, null);
            _profileItemColor = new ColorPreferenceItem(builder.DefineStringValue(_profileDefinition, "profileItemColor", "Black", null), KnownColor.Black);
            _description = builder.DefineStringValue(_profileDefinition, "description", "", null);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void SaveToPreference() {
            // 一度も読み込まれていない場合は読み込む(フォームが一度も表示されてない場合に設定が消滅してしまう)
            if (this.PreferenceLoaded != true) this.LoadFromPreference();
            IPreferenceFolderArray fa = _rootPreference.FindChildFolderArray(_profileDefinition.Id);
            fa.Clear();

            foreach (ConnectProfileStruct prof in ConnectProfilePlugin.Profiles) {
                IPreferenceFolder f = fa.CreateNewFolder();

                // パスワード暗号化(キーはPLUGIN_ID)
                string pw = "";
                string supw = "";
                if (prof.Password != "") pw = EncryptString(prof.Password, ConnectProfilePlugin.PLUGIN_ID);
                if (prof.SUPassword != "") supw = EncryptString(prof.SUPassword, ConnectProfilePlugin.PLUGIN_ID);

                // 値代入
                fa.ConvertItem(f, _hostName).AsString().Value = prof.HostName;
                fa.ConvertItem(f, _protocol).AsString().Value = prof.Protocol.ToString();
                fa.ConvertItem(f, _port).AsInt().Value = prof.Port;
                fa.ConvertItem(f, _authType).AsString().Value = prof.AuthType.ToString();
                fa.ConvertItem(f, _keyFile).AsString().Value = prof.KeyFile;
                fa.ConvertItem(f, _userName).AsString().Value = prof.UserName;
                fa.ConvertItem(f, _password).AsString().Value = (pw != null) ? pw : "";
                fa.ConvertItem(f, _autoLogin).AsBool().Value = prof.AutoLogin;
                fa.ConvertItem(f, _loginPrompt).AsString().Value = prof.LoginPrompt;
                fa.ConvertItem(f, _passwordPrompt).AsString().Value = prof.PasswordPrompt;
                fa.ConvertItem(f, _execCommand).AsString().Value = prof.ExecCommand;
                fa.ConvertItem(f, _suUserName).AsString().Value = prof.SUUserName;
                fa.ConvertItem(f, _suPassword).AsString().Value = (supw != null) ? supw : "";
                fa.ConvertItem(f, _suType).AsString().Value = prof.SUType;
                fa.ConvertItem(f, _charCode).AsString().Value = prof.CharCode.ToString();
                fa.ConvertItem(f, _newLine).AsString().Value = prof.NewLine.ToString();
                fa.ConvertItem(f, _telnetNewLine).AsBool().Value = prof.TelnetNewLine;
                fa.ConvertItem(f, _terminalType).AsString().Value = prof.TerminalType.ToString();
                fa.ConvertItem(f, _terminalFontColor.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ForeColor.ToArgb(), 16);
                fa.ConvertItem(f, _terminalBGColor.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.BackColor.ToArgb(), 16);
                fa.ConvertItem(f, _terminalESCColor0.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ESColorSet[0].Color.ToArgb(), 16);
                fa.ConvertItem(f, _terminalESCColor1.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ESColorSet[1].Color.ToArgb(), 16);
                fa.ConvertItem(f, _terminalESCColor2.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ESColorSet[2].Color.ToArgb(), 16);
                fa.ConvertItem(f, _terminalESCColor3.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ESColorSet[3].Color.ToArgb(), 16);
                fa.ConvertItem(f, _terminalESCColor4.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ESColorSet[4].Color.ToArgb(), 16);
                fa.ConvertItem(f, _terminalESCColor5.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ESColorSet[5].Color.ToArgb(), 16);
                fa.ConvertItem(f, _terminalESCColor6.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ESColorSet[6].Color.ToArgb(), 16);
                fa.ConvertItem(f, _terminalESCColor7.PreferenceItem).AsString().Value = Convert.ToString(prof.RenderProfile.ESColorSet[7].Color.ToArgb(), 16);
                fa.ConvertItem(f, _terminalAsciiFont).AsString().Value = prof.RenderProfile.FontName.ToString();
                fa.ConvertItem(f, _terminalCjkFont).AsString().Value = prof.RenderProfile.CJKFontName.ToString();
                fa.ConvertItem(f, _terminalFontSize).AsInt().Value = (int)prof.RenderProfile.FontSize;
                fa.ConvertItem(f, _terminalClearType).AsBool().Value = prof.RenderProfile.UseClearType;
                fa.ConvertItem(f, _terminalBoldStyle).AsBool().Value = prof.RenderProfile.EnableBoldStyle;
                fa.ConvertItem(f, _terminalForceBoldStyle).AsBool().Value = prof.RenderProfile.ForceBoldStyle;
                fa.ConvertItem(f, _terminalBGImage).AsString().Value = prof.RenderProfile.BackgroundImageFileName;
                fa.ConvertItem(f, _terminalBGImagePos).AsString().Value = prof.RenderProfile.ImageStyle.ToString();
                fa.ConvertItem(f, _commandSendInterval).AsInt().Value = prof.CommandSendInterval;
                fa.ConvertItem(f, _promptRecvTimeout).AsInt().Value = prof.PromptRecvTimeout;
                fa.ConvertItem(f, _profileItemColor.PreferenceItem).AsString().Value = Convert.ToString(prof.ProfileItemColor.ToArgb(), 16);
                fa.ConvertItem(f, _description).AsString().Value = prof.Description;
            }
        }

        /// <summary>
        /// 読み込み
        /// </summary>
        public void LoadFromPreference() {
            IPreferenceFolderArray fa = _rootPreference.FindChildFolderArray(_profileDefinition.Id);

            foreach (IPreferenceFolder f in fa.Folders) {
                ConnectProfileStruct prof = new ConnectProfileStruct();
                prof.HostName = fa.ConvertItem(f, _hostName).AsString().Value;
                if (fa.ConvertItem(f, _protocol).AsString().Value == "Telnet") prof.Protocol = ConnectionMethod.Telnet;
                else if (fa.ConvertItem(f, _protocol).AsString().Value == "SSH1") prof.Protocol = ConnectionMethod.SSH1;
                else if (fa.ConvertItem(f, _protocol).AsString().Value == "SSH2") prof.Protocol = ConnectionMethod.SSH2;
                prof.Port = fa.ConvertItem(f, _port).AsInt().Value;
                if (fa.ConvertItem(f, _authType).AsString().Value == "Password") prof.AuthType = AuthType.Password;
                else if (fa.ConvertItem(f, _authType).AsString().Value == "PublicKey") prof.AuthType = AuthType.PublicKey;
                else if (fa.ConvertItem(f, _authType).AsString().Value == "KeyboardInteractive") prof.AuthType = AuthType.KeyboardInteractive;
                prof.KeyFile = fa.ConvertItem(f, _keyFile).AsString().Value;
                prof.UserName = fa.ConvertItem(f, _userName).AsString().Value;
                prof.Password = fa.ConvertItem(f, _password).AsString().Value;
                prof.AutoLogin = fa.ConvertItem(f, _autoLogin).AsBool().Value;
                prof.LoginPrompt = fa.ConvertItem(f, _loginPrompt).AsString().Value;
                prof.PasswordPrompt = fa.ConvertItem(f, _passwordPrompt).AsString().Value;
                prof.ExecCommand = fa.ConvertItem(f, _execCommand).AsString().Value;
                prof.SUUserName = fa.ConvertItem(f, _suUserName).AsString().Value;
                prof.SUPassword = fa.ConvertItem(f, _suPassword).AsString().Value;
                prof.SUType = fa.ConvertItem(f, _suType).AsString().Value;
                if (fa.ConvertItem(f, _charCode).AsString().Value == "ISO8859_1") prof.CharCode = EncodingType.ISO8859_1;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "UTF8") prof.CharCode = EncodingType.UTF8;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "EUC_JP") prof.CharCode = EncodingType.EUC_JP;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "SHIFT_JIS") prof.CharCode = EncodingType.SHIFT_JIS;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "GB2312") prof.CharCode = EncodingType.GB2312;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "BIG5") prof.CharCode = EncodingType.BIG5;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "EUC_CN") prof.CharCode = EncodingType.EUC_CN;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "EUC_KR") prof.CharCode = EncodingType.EUC_KR;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "UTF8_Latin") prof.CharCode = EncodingType.UTF8_Latin;
                else if (fa.ConvertItem(f, _charCode).AsString().Value == "OEM850") prof.CharCode = EncodingType.OEM850;
                if (fa.ConvertItem(f, _newLine).AsString().Value == "CR") prof.NewLine = NewLine.CR;
                else if (fa.ConvertItem(f, _newLine).AsString().Value == "LF") prof.NewLine = NewLine.LF;
                else if (fa.ConvertItem(f, _newLine).AsString().Value == "CRLF") prof.NewLine = NewLine.CRLF;
                prof.TelnetNewLine = fa.ConvertItem(f, _telnetNewLine).AsBool().Value;
                if (fa.ConvertItem(f, _terminalType).AsString().Value == "KTerm") prof.TerminalType = TerminalType.KTerm;
                else if (fa.ConvertItem(f, _terminalType).AsString().Value == "VT100") prof.TerminalType = TerminalType.VT100;
                else if (fa.ConvertItem(f, _terminalType).AsString().Value == "XTerm") prof.TerminalType = TerminalType.XTerm;
                else if (fa.ConvertItem(f, _terminalType).AsString().Value == "XTerm256Color") prof.TerminalType = TerminalType.XTerm256Color;
                prof.CommandSendInterval = fa.ConvertItem(f, _commandSendInterval).AsInt().Value;
                prof.PromptRecvTimeout = fa.ConvertItem(f, _promptRecvTimeout).AsInt().Value;
                prof.ProfileItemColor = Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _profileItemColor.PreferenceItem).AsString().Value, Color.Black);
                prof.Description = fa.ConvertItem(f, _description).AsString().Value;

                // 表示オプション
                prof.RenderProfile = ConnectProfilePlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions.CreateRenderProfile();
                prof.RenderProfile.ForeColor = Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalFontColor.PreferenceItem).AsString().Value, Color.White);
                prof.RenderProfile.BackColor = Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalBGColor.PreferenceItem).AsString().Value, Color.Black);
                prof.RenderProfile.ESColorSet = new EscapesequenceColorSet();
                prof.RenderProfile.ESColorSet.ResetToDefault();
                prof.RenderProfile.ESColorSet[0] = new ESColor(Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalESCColor0.PreferenceItem).AsString().Value, Color.Black), false);
                prof.RenderProfile.ESColorSet[1] = new ESColor(Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalESCColor1.PreferenceItem).AsString().Value, Color.Red), false);
                prof.RenderProfile.ESColorSet[2] = new ESColor(Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalESCColor2.PreferenceItem).AsString().Value, Color.Green), false);
                prof.RenderProfile.ESColorSet[3] = new ESColor(Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalESCColor3.PreferenceItem).AsString().Value, Color.Yellow), false);
                prof.RenderProfile.ESColorSet[4] = new ESColor(Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalESCColor4.PreferenceItem).AsString().Value, Color.Blue), false);
                prof.RenderProfile.ESColorSet[5] = new ESColor(Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalESCColor5.PreferenceItem).AsString().Value, Color.Magenta), false);
                prof.RenderProfile.ESColorSet[6] = new ESColor(Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalESCColor6.PreferenceItem).AsString().Value, Color.Cyan), false);
                prof.RenderProfile.ESColorSet[7] = new ESColor(Poderosa.ParseUtil.ParseColor(fa.ConvertItem(f, _terminalESCColor7.PreferenceItem).AsString().Value, Color.White), false);
                prof.RenderProfile.FontName = fa.ConvertItem(f, _terminalAsciiFont).AsString().Value;
                prof.RenderProfile.CJKFontName = fa.ConvertItem(f, _terminalCjkFont).AsString().Value;
                prof.RenderProfile.FontSize = fa.ConvertItem(f, _terminalFontSize).AsInt().Value;
                prof.RenderProfile.UseClearType = fa.ConvertItem(f, _terminalClearType).AsBool().Value;
                prof.RenderProfile.EnableBoldStyle = fa.ConvertItem(f, _terminalBoldStyle).AsBool().Value;
                prof.RenderProfile.ForceBoldStyle = fa.ConvertItem(f, _terminalForceBoldStyle).AsBool().Value;
                prof.RenderProfile.BackgroundImageFileName = fa.ConvertItem(f, _terminalBGImage).AsString().Value;
                if (fa.ConvertItem(f, _terminalBGImagePos).AsString().Value == "Center") prof.RenderProfile.ImageStyle = ImageStyle.Center;
                else if (fa.ConvertItem(f, _terminalBGImagePos).AsString().Value == "TopLeft") prof.RenderProfile.ImageStyle = ImageStyle.TopLeft;
                else if (fa.ConvertItem(f, _terminalBGImagePos).AsString().Value == "TopRight") prof.RenderProfile.ImageStyle = ImageStyle.TopRight;
                else if (fa.ConvertItem(f, _terminalBGImagePos).AsString().Value == "BottomLeft") prof.RenderProfile.ImageStyle = ImageStyle.BottomLeft;
                else if (fa.ConvertItem(f, _terminalBGImagePos).AsString().Value == "BottomRight") prof.RenderProfile.ImageStyle = ImageStyle.BottomRight;
                else if (fa.ConvertItem(f, _terminalBGImagePos).AsString().Value == "Scaled") prof.RenderProfile.ImageStyle = ImageStyle.Scaled;
                else if (fa.ConvertItem(f, _terminalBGImagePos).AsString().Value == "HorizontalFit") prof.RenderProfile.ImageStyle = ImageStyle.HorizontalFit;
                else if (fa.ConvertItem(f, _terminalBGImagePos).AsString().Value == "VerticalFit") prof.RenderProfile.ImageStyle = ImageStyle.VerticalFit;

                // パスワード複合化
                if (prof.Password != "") prof.Password = DecryptString(prof.Password, ConnectProfilePlugin.PLUGIN_ID);
                if (prof.SUPassword != "") prof.SUPassword = DecryptString(prof.SUPassword, ConnectProfilePlugin.PLUGIN_ID);

                // プロファイル追加
                ConnectProfilePlugin.Profiles.AddProfile(prof);
            }

            _preferenceLoaded = true;
        }

        /// <summary>
        /// 文字列を暗号化
        /// </summary>
        /// <param name="sourceString">暗号化文字列</param>
        /// <param name="password">暗号化パスワード</param>
        /// <returns>暗号化後文字列</returns>
        public string EncryptString(string sourceString, string password) {
            byte[] key, iv, strBytes, encBytes;
            System.Security.Cryptography.RijndaelManaged rijndael;
            System.Security.Cryptography.ICryptoTransform encryptor;

            try {
                rijndael = new System.Security.Cryptography.RijndaelManaged();
                GenerateKeyFromPassword(password, rijndael.KeySize, out key, rijndael.BlockSize, out iv, 100);
                rijndael.Key = key;
                rijndael.IV = iv;
                strBytes = System.Text.Encoding.UTF8.GetBytes(sourceString);
                encryptor = rijndael.CreateEncryptor();
                encBytes = encryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
                encryptor.Dispose();
            } catch (Exception) {
                throw;
            }

            return System.Convert.ToBase64String(encBytes);
        }

        /// <summary>
        /// 文字列を復号化
        /// </summary>
        /// <param name="sourceString">暗号化文字列</param>
        /// <param name="password">パスワード</param>
        /// <returns>復号化後文字列</returns>
        public string DecryptString(string sourceString, string password) {
            byte[] key, iv, strBytes, decBytes;
            System.Security.Cryptography.RijndaelManaged rijndael;
            System.Security.Cryptography.ICryptoTransform decryptor;

            // 複合化(コンバートあり)
            try {
                // v2.0
                rijndael = new System.Security.Cryptography.RijndaelManaged();
                GenerateKeyFromPassword(password, rijndael.KeySize, out key, rijndael.BlockSize, out iv, 100);
                rijndael.Key = key;
                rijndael.IV = iv;
                strBytes = System.Convert.FromBase64String(sourceString);
                decryptor = rijndael.CreateDecryptor();
                decBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
                decryptor.Dispose();
            } catch (Exception) {
                try {
                    // v1.0-1.3
                    rijndael = new System.Security.Cryptography.RijndaelManaged();
                    GenerateKeyFromPassword(password, rijndael.KeySize, out key, rijndael.BlockSize, out iv, 1000);
                    rijndael.Key = key;
                    rijndael.IV = iv;
                    strBytes = System.Convert.FromBase64String(sourceString);
                    decryptor = rijndael.CreateDecryptor();
                    decBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
                    decryptor.Dispose();
                } catch (Exception) {
                    throw;
                }
            }

            return System.Text.Encoding.UTF8.GetString(decBytes);
        }

        /// <summary>
        /// パスワードから共有キー/初期化ベクタを生成
        /// </summary>
        /// <param name="password">パスワード</param>
        /// <param name="keySize">共有キーサイズ(ビット)</param>
        /// <param name="key">共有キー</param>
        /// <param name="blockSize">初期化ベクタサイズ(ビット)</param>
        /// <param name="iv">初期化ベクタ</param>
        /// <param name="iterationCnt">反復処理回数</param>
        private void GenerateKeyFromPassword(string password, int keySize, out byte[] key, int blockSize, out byte[] iv, int iterationCnt) {
            // パスワードから共有キーと初期化ベクタを作成(salt決定)
            byte[] salt = System.Text.Encoding.UTF8.GetBytes("u+-J$ejeP/+%5lDe9_oVk#Q4p/cchi3C");

            // Rfc2898DeriveBytesオブジェクト作成
            System.Security.Cryptography.Rfc2898DeriveBytes deriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt);

            // 反復処理回数
            deriveBytes.IterationCount = iterationCnt;

            // 共有キー/初期化ベクタを生成
            key = deriveBytes.GetBytes(keySize / 8);
            iv = deriveBytes.GetBytes(blockSize / 8);
        }

        /// <summary>
        /// ValidateFolder
        /// </summary>
        public void ValidateFolder(IPreferenceFolder folder, IPreferenceValidationResult output) {
            return;
        }

        /// <summary>
        /// QueryAdapter
        /// </summary>
        public object QueryAdapter(IPreferenceFolder folder, Type type) {
            return null;
        }

        /// <summary>
        /// プラグインID
        /// </summary>
        public string PreferenceID {
            get { return ConnectProfilePlugin.PLUGIN_ID; }
        }

        /// <summary>
        /// 読み込み完了フラグ
        /// </summary>
        public bool PreferenceLoaded {
            get { return _preferenceLoaded; }
        }
    }
}
