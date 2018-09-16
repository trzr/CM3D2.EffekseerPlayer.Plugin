CM3D2.EffekseerPlayer.Plugin
---

[Effekseer][]で作成されたエフェクトをCM3D2/COM3D2で再生するためのプラグインです。  
Effekseerの再生部分は[EffekseerForUnity][]のサンプルコードを元にしています。  

---
## ■ 導入
#### ◇前提条件  
UnityInjectorが導入済みであること。

#### ◇動作確認環境

| env    | version | plugin version    |
| -------| ------- | ----------------- |
| CM3D2  | 1.57    | 0.1.0 - 0.2.6     |
| COM3D2 | 1.19.1  | 0.1.0 -           |

  - 前提プラグイン：UnityInjector/Sybaris  
  ※ COM3D2ではSybaris 2系を想定

想定するEffekseerバージョンの対応は以下の通り。

| plugin-version | effekseer version|
| -------------- | ---------------- |
| - 0.2.6        | 1.32             |
| 0.3.0 -        | 1.41             |

#### ◇インストール  

[Releases][]ページから、対応するバージョンのzipファイルをダウンロードし、  
解凍した後、UnityInjectorフォルダ以下をプラグインフォルダにコピーしてください。  
CM3D2とCOM3D2でそれぞれ別のdllファイルになります。  
また、 **EffekseerUnity.dll** を以下のフォルダに配置してください。  
  
~~~
{CM3D2インストールフォルダ}\CM3D2x64_Data\Plugins\     CM3D2の場合
{COM3D2インストールフォルダ}\COM3D2x64_Data\Plugins\   COM3D2の場合
~~~

#### ◇ビルド方法  

自身でビルドする場合は、
* compile_cm3d2.bat
* compile_com3d2.bat
を実行することで、それぞれdllを生成できます。

ただし、前提条件は以下の通りです。
 * CM3D2 or COM3D2のインストールパスがレジストリに存在 or 直接バッチに指定
 * Sybarisのパスが設定されている

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

* プリセットファイル(レシピファイル)のパス  
CM3D2/COM3D2上で設定した再生レシピはJSON形式で以下に保存されます。
~~~
{CM3D2インストールフォルダ}\Sybaris\Plugins\UnityInjector\Config\efk\_recipes\  CM3D2の場合
{COM3D2インストールフォルダ}\Sybaris\UnityInjector\Config\efk\_recipes\         COM3D2の場合
~~~

* [iniファイル][] の配置パス  
~~~
{CM3D2インストールフォルダ}\Sybaris\Plugins\UnityInjector\Config\EffekseerPlayerPlugin.ini
{COM3D2インストールフォルダ}\Sybaris\UnityInjector\Config\EffekseerPlayerPlugin.ini
~~~

[iniファイル][]の各設定項目や [画面][]については、[wiki][]を参照してください。


#### ◇補足
* Effect音の再生  
Effekseerプロジェクト上ではwavファイルを指定していても、  
そのwavを配置すべきパスにoggファイルを配置すれば読み込むことができます。  
(wavがない場合にoggを探すよう動作します)


#### ◇その他
Known IssuesとTODOは、[memo][]ページを参照。

#### ◇License
本プロジェクトのコードやリソースは、MITライセンスとなります。

* Effekseer/以下のコード  
は、[EffekseerForUnity][] のサンプルコードを改造したものです。  
また、同梱の **EffekseerUnity.dll** は下記のMITライセンスとなります。  
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
[wiki]:https://github.com/trzr/CM3D2.EffekseerPlayer.Plugin/wiki
[memo]:https://github.com/trzr/CM3D2.EffekseerPlayer.Plugin/wiki/memo
[iniファイル]:https://github.com/trzr/CM3D2.EffekseerPlayer.Plugin/wiki/ini
[画面]:https://github.com/trzr/CM3D2.EffekseerPlayer.Plugin/wiki/画面説明
