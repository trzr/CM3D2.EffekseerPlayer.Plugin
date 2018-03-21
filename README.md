CM3D2.EffekseerPlayer.Plugin
---

[Effekseer][]で作成されたエフェクトをCM3D2/COM3D2で再生するためのプラグインです。  
Effekseerの再生部分は[EffekseerForUnity][]のサンプルコードを元にしています。  

---
## ■ 導入
#### ◇前提条件  
UnityInjectorが導入済みであること。

#### ◇動作確認環境
  - バージョン：**1.57** (CM3D2), **1.07** (COM3D2)    
  - 前提プラグイン：UnityInjector/Sybaris  
  ※ COM3D2ではSybaris 2系を想定
  - Effekseerバージョン: **1.32**

#### ◇インストール  

[Releases][]ページから、対応するバージョンのzipファイルをダウンロードし、  
解凍した後、UnityInjectorフォルダ以下をプラグインフォルダにコピーしてください。  
CM3D2とCOM3D2でそれぞれ別のdllファイルになります。  
また、 **EffekseerUnity.dll** を以下のフォルダに配置してください。  
  
~~~
{CM3D2インストールフォルダ}\CM3D2x64_Data\Plugins\     CM3D2の場合
{COM3D2インストールフォルダ}\COM3D2x64_Data\Plugins\   COM3D2の場合
~~~

#### ◇使い方  
(以降のパスはSybarisを導入している場合を例にしています)  
Effekseerで作成したエフェクトをCM3D2上で再生するためには、  
.efkファイル(.bytesファイル)としてエクスポートする必要があります。  

* efk/bytesファイルの配置先
~~~
{CM3D2インストールフォルダ}\Sybaris\Plugins\UnityInjector\Config\efk\  CM3D2の場合
{COM3D2インストールフォルダ}\Sybaris\UnityInjector\Config\efk\         COM3D2の場合
~~~
  ※ このパスはiniファイルでも変更可能です。  
   efk/bytesファイルは、上記フォルダ直下と、一段下のフォルダ内のみを走査します。  
   また、efk/bytesに設定された画像や音ファイルは、  
   efk/bytesファイルの位置を基準とした相対パスで配置してください。  

* プリセットファイル(レシピファイル)  
CM3D2/COM3D2上で設定した再生レシピはJSON形式で以下に保存されます。
~~~
{CM3D2インストールフォルダ}\Sybaris\Plugins\UnityInjector\Config\efk\_recipes\  CM3D2の場合
{COM3D2インストールフォルダ}\Sybaris\UnityInjector\Config\efk\_recipes\         COM3D2の場合
~~~

* iniファイル  
~~~
{CM3D2インストールフォルダ}\Sybaris\Plugins\UnityInjector\Config\EffekseerPlayerPlugin.ini
{COM3D2インストールフォルダ}\Sybaris\UnityInjector\Config\EffekseerPlayerPlugin.ini
~~~

#### ◇補足
* Effect音の再生
Effekseerプロジェクト上ではwavファイルを指定していても、  
そのwavを配置すべきパスにoggファイルを配置すれば読み込むことができます。  
(wavがない場合にoggを探すよう動作します)

#### ◇Known Issues
* エンドフレームを超えて再生され続けるEffectがある(Effekseer側の仕様？)
* メインウィンドウのツリーでリピートを変更しても保存しない

#### ◇TODOリスト
* targetLocationの指定
* 位置指定：3D上の矢印操作
* 回転指定：3D上の回転操作
* 再生ディレイの設定(N msec or frame 後に再生開始)
* 再生時間の設定 (N msec or frame 後に自動停止)
  (エンドフレームの指定)
* AssetBundleファイルを指定してエフェクトのロード
* アタッチ先のバリエーション追加
  * 存在するメイドすべてに複製してアタッチ (/複数のメイド選択)
  * 特定のアイテムが装着されていた場合のみアタッチ など
* UI周りの改良 
  * 削除操作時の確認表示
  * 同名設定上書き時の確認表示
  * 再読み込み時の確認表示
  * UNDO/REDO
  * efk再読み込みボタン
  * efkコンボのフィルタリング
  * 再生設定レシピの変更フラグ(dirty チェック)
  * レシピファイルに指定されたefkが参照できない場合の画面上の表示(グレーアウトなど)
  * アイテム未選択時のボタン操作の無効化
  * 座標の相対位置保持モード(回転を無視した相対位置)


#### ◇License
本プロジェクトのコードやリソースは、MITライセンスです。

* Effekseer/以下のコード  
は、[EffekseerForUnity][] のサンプルコードを改造したものです。  
また、同梱DLLは以下のMITライセンスとなります。  
* EffekseerUnity.dll  
~~~
Copyright (c) 2011 Effekseer Project
Released under the MIT License
https://github.com/effekseer/Effekseer/blob/master/LICENSE
~~~

* GUIまわりのコードは、ChangeMotionPluginのコードを一部参考にさせていただきました。感謝。  

-----

[Releases]:https://github.com/trzr/CM3D2.EffekseerPlayer.Plugin/releases
[Effekseer]:https://github.com/effekseer/Effekseer
[EffekseerForUnity]:https://github.com/effekseer/EffekseerForUnity
