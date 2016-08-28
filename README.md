# Poderosa 接続プロファイルプラグイン (v2.0)
※日本語から英語への翻訳はBing翻訳を使用しています。  
(* Japanese to English translations use the Bing translator.)

※プラグインDLLは[Download & History](#dl_history)または[release][MENU-RELEASE]からダウンロードできます。  
(* The plug-in DLL [Download & History](#dl_history) or can be downloaded from [release][MENU-RELEASE].)

Poderosaのプラグインです。(接続プロファイルプラグイン)  
(This plugin is for Poderosa. connection profile plugin.)

このプラグインは、接続先ホストをプロファイル管理するものです。  
(This plugin is a connected host to manage profile.)

VisualC# 2013で開発し、Poderosa v4.3.16(.Net4.5と2.0)で動作確認しています。  
(Developed in VisualC# 2013, has confirmed on the Poderosa v4.3.16(.Net4.5 and 2.0).)


## Features
* 各項目を入力して接続先ホストをプロファイル管理します  
(Enter each item, manages the connected host profile.)

* アクティブセッションを接続プロファイルに直接追加できます(v1.3以降)  
(Active session you can add directly in the connection profile.)

* プロファイル操作は、接続/追加/編集/削除/コピー/CSVエクスポート/CSVインポートができます  
(Profile operation makes connections / add / edit / delete / copy / CSV export / CSV import.)

* CSVエクスポートする場合はパスワードを削除して保存するかどうか選択できます。  
(You can choose whether or not to save the CSV export if you remove password.)

* プロファイルリストはフィルタリングを行うことができます  
(Profile you can do filtering.)

* フィルタテキストボックス内で上下キーを押下するとプロファイルリストビューにフォーカスを移すことができます(クイック選択)  
(You can press the up and down keys in the filter text box and give the focus to the "profile list view". (Quick select))

* プロファイルを複数選択することで選択ホストへの連続接続ができます  
(To select multiple profiles that enables continuous connection to the selected host.)

* 自動ログイン機能を搭載しています (主にTelnetで活用)  
(Includes auto-login feature. (Mainly used in Telnet))

* 自動ログイン後にSUスイッチができます  
(Automatic login then you can SU switch.)

* 自動ログイン後に指定したコマンドの実行ができます  
(You can run the command specified in the auto login after.)

* プロファイル毎にリストの項目色とターミナルの背景色/フォント色/フォント/エスケープシーケンス色/背景画像を設定できます  
(開発環境/本番環境などで設定を使い分ける用途を想定)  
(You can set profile every list item colors, and background color, font color, font name, escape sequences color, background images of Terminal.)  
(To use different settings, the development environment and the production environment)

* プロファイルデータはoptions.confに保存されます (パスワードは暗号化保存)  
(Profile data is stored in options.conf. (Passwords are stored encrypted))

* [ツール] - [オプション] - [コマンド]でキーバンド設定が出来ます  
(Can the KeyBinding settings in the [Tools] - [Options] - [Command])


## Installation
Poderosaディレクトリ内に下記のように配置します。  
(Poderosa directory in the position as shown below.)

`Poderosa/`  
`├── ConnectProfile`  
`│   ├── Contrib.ConnectProfile.dll`  
`│   └── Contrib.ConnectProfile.pdb`  
`└── Poderosa.exe, and other files.`


## Usage
※表示言語はPoderosaの言語設定に準拠しています (画像は英語版)  
(* Language conforms to the language setting of the Poderosa (the image is the English version))

1. ツールメニューからTelnet/SSH接続プロファイルを選択します。  
(Select the Telnet/SSH connection profiles from the Tools menu.)  
![UsageImg1](https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/wiki/img/img1.png)

2. 追加ボタンを押下して必要な情報を入力します。  
(Press the Add button and enter the required information.)  
![UsageImg2](https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/wiki/img/img2.png)

3. 作成したプロファイルを選択してOKボタンを押下して接続します。  
(Select the profile that you just created and press the OK button to connect.)  
![UsageImg3](https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/wiki/img/img3.png)

4. 必要に応じてキーバンド設定を行います。  
(Optionally configure key band.)  
![UsageImg4](https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/wiki/img/img4.png)


## <a name ="dl_history">Download & History
* 2016/08/28 v2.0 [(.Net2.0)][DL-2.0-net20] [(.Net4.5)][DL-2.0-net45]  
**※重要：v2.0プロファイル仕様が変更されているため、必ず「[PODEROSA_DIR]/options.conf」をバックアップ後にバージョンアップしてください。万一プロファイルデータが損失してしまった場合はダウングレードしてください。**  
**(*Important: Please upgrade to v2.0 Profiles specification has changed, always backup the "[PODEROSA_DIR]/options.conf" after. If you lost the profile data should try to downgrade.)**  
プロファイル編集画面に「表示オプション」を追加しました。（背景色、フォント色、フォント名、エスケープシーケンス色、背景画像を設定することができます）  
(Added "show options" on the ProfileEdit Form. (You can set the background color, font color, font name, escape sequences color, background images.))  
パスワード表示オプションを追加しました。（この設定は保存されません）  
(Password display option was added. (This setting is not saved.))  
パスワードプロンプト項目を任意設定にしました。  
(Password prompt item in any setting.)  
接続プロファイルのダイアログサイズを変更可能にしました。  
(You can change the connection profiles dialog size.)  
プロファイル削除時に確認メッセージを表示するようにしました。  
(During profile deletion confirmation message is shown.)  
SUコマンド項目に「sudo su」と「sudo su -」を追加しました。  
(SU command item 'sudo su' and ' sudo su-' has been added.)  
プロファイルの初回読み込みを高速化しました。  
(Profile initial load faster.)  
接続プロファイルダイアログに「コンソール表示サンプル」を表示するようにしました。（背景色、フォント色、ASCIIフォント、フォントサイズが適用されます）  
(Connection profile dialog added to sample the console display. (Background color, font color, ASCII font, the font size will be applied))  
文字列リソースを修正しました。  
(Fixed a string resource.)

* 2016/06/24 v1.3 [(.Net2.0)][DL-1.3-net20] [(.Net4.5)][DL-1.3-net45]  
メインメニューまたはコンテキストメニューからアクティブセッションを接続プロファイルに追加する機能を追加しました。  
(Added the ability to add active session connection profile from the main menu or the context menu.)  
プラグインの登録処理を修正しました。  
(Fixed plug-in registration process.)

* 2016/02/15 v1.2 [(.Net2.0)][DL-1.2-net20] [(.Net4.5)][DL-1.2-net45]  
プロファイルリストアイテムが空の場合にクイック選択を使用するとエラーが発生してしまう不具合を修正しました。  
(Fixed bug if using quick select profile list item and it fails.)

* 2016/02/08 v1.1 [(.Net2.0)][DL-1.1-net20] [(.Net4.5)][DL-1.1-net45]  
フィルタテキストボックスにフォーカスがある場合、上下キーで「プロファイルリストビュー」をアクティブにする際の挙動を変更しました。  
(Changed the behavior to activate "profile list view" up/down keys when focus is in the filter text box.)  
文字列リソースを修正しました。  
(Fixed a string resource.)

* 2015/10/29 v1.0 [(.Net2.0)][DL-1.0-net20] [(.Net4.5)][DL-1.0-net45]  
初期リリース  
(Initial release.)


## License
Copyright 2015-2016 yoshikixxxx. ([Twitter][TWITTER])  
Licensed under the Apache License, Version 2.0 (the "License");  
you may not use this file except in compliance with the License.




[MENU-RELEASE]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases
[DL-1.0-net20]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.0/connectprofile_1.0_net20.zip
[DL-1.0-net45]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.0/connectprofile_1.0_net45.zip
[DL-1.1-net20]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.1/connectprofile_1.1_net20.zip
[DL-1.1-net45]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.1/connectprofile_1.1_net45.zip
[DL-1.2-net20]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.2/connectprofile_1.2_net20.zip
[DL-1.2-net45]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.2/connectprofile_1.2_net45.zip
[DL-1.3-net20]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.3/connectprofile_1.3_net20.zip
[DL-1.3-net45]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.3/connectprofile_1.3_net45.zip
[DL-2.0-net20]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/2.0/connectprofile_2.0_net20.zip
[DL-2.0-net45]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/2.0/connectprofile_2.0_net45.zip
[TWITTER]: https://twitter.com/yoshikixxxxaol
