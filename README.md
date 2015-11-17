# Poderosa ConnectProfile plugin
※日本語から英語への翻訳はBing翻訳を使用しています。  
(* Japanese to English translations use the Bing translator.)

※プラグインDLLは[Download & History](#dl_history)または[release][MENU-RELEASE]からダウンロードできます。  
(* The plug-in DLL [Download & History](#dl_history) or can be downloaded from [release][MENU-RELEASE].)

Poderosaのプラグインです。(接続プロファイルプラグイン)  
(This plugin is for Poderosa. connection profile plugin.)

このプラグインは、接続先ホストをプロファイル管理するものです。  
(This plugin is a connected host to manage profile.)

VisualC# 2013で開発し、Poderosa v4.3.16(.Net4.5)で動作確認しています。  
(Developed in VisualC# 2013, has confirmed on the Poderosa v4.3.16(.Net4.5).)


## Features
* 各項目を入力して接続先ホストをプロファイル管理します  
(Enter each item, manages the connected host profile.)

* プロファイル操作は、接続/追加/編集/削除/コピー/CSVエクスポート/CSVインポートができます  
(Profile operation makes connections / add / edit / delete / copy / CSV export / CSV import.)

* CSVエクスポートする場合はパスワードを削除して保存するかどうか選択できます。  
(You can choose whether or not to save the CSV export if you remove password.)

* プロファイルリストはフィルタリングを行うことができます  
(Profile you can do filtering.)

* プロファイルを複数選択することで選択ホストへの連続接続ができます  
(To select multiple profiles that enables continuous connection to the selected host.)

* 自動ログイン機能を搭載しています (主にTelnetで活用)  
(Includes auto-login feature. (Mainly used in Telnet))

* 自動ログイン後にSUスイッチができます  
(Automatic login then you can SU switch.)

* 自動ログイン後に指定したコマンドの実行ができます  
(You can run the command specified in the auto login after.)

* プロファイル毎にリストの項目色とターミナルの背景色/フォント色を設定できます  
(開発環境/本番環境などで設定を使い分ける用途を想定)  
(You can set profile every list item colors and background color and font color of Terminal.)  
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
* 2015/10/29 [v1.0][DL-1.0]  
初期リリース  
(Initial release.)


## License
Copyright 2015 yoshikixxxx.  
Licensed under the Apache License, Version 2.0 (the "License");  
you may not use this file except in compliance with the License.




[MENU-RELEASE]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases
[DL-1.0]: https://github.com/yoshikixxxx/poderosa-connectprofile-plugin/releases/download/1.0/connectprofile_1.0.zip
