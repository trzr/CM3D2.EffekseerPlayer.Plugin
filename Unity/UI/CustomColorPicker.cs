using System;
using System.Globalization;
using EffekseerPlayer.Unity.Data;
using EffekseerPlayer.Unity.Util;
using EffekseerPlayer.Util;
using UnityEngine;

namespace EffekseerPlayer.Unity.UI {
    /// <summary>
    /// カスタムカラーピッカー.
    /// カラーマップのテクスチャ(mapBaseTex)と、輝度テクスチャ(lightTex)が適切に設定される前提とする.
    ///
    /// 外部からの設定項目
    /// lightBorderWidth: lightTexの縁サイズ
    /// </summary>
    public class CustomColorPicker : GUIControl {
        #region Methods
        public CustomColorPicker(GUIControl parent, EditColorTex etc, ColorPresetManager presetMgr=null)
            : base(parent) {

            this.presetMgr = presetMgr;
            texHeight = Math.Max(etc.lightTex.height, etc.mapBaseTex.height);
            EditVal = etc;
        }

        public override void Awake() {
            if (TextFStyle == null) {
                TextFStyle = new GUIStyle("textField") {
                    alignment = TextAnchor.MiddleCenter,
                    normal = {textColor = TextColor},
                };
                if (fontSize > 0) {
                    TextFStyle.fontSize = fontSize;
                }
            }
            if (ButtonStyle == null) {
                ButtonStyle = new GUIStyle("button") {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSizeS,
                };
            }
            if (PresetButtonStyle == null) {
                PresetButtonStyle = new GUIStyle("button") {
                    margin = new RectOffset(2, 2, 1, 1),
                    padding = new RectOffset(1, 1, 1, 1),
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = fontSizeS,
                };
            }

            if (PresetIconStyle == null) {
                PresetIconStyle = new GUIStyle("label") {
                    contentOffset = new Vector2(0, 1),
                    margin = new RectOffset(1, 1, 1, 1),
                    padding = new RectOffset(1, 1, 1, 1)
                };
            }

            if (LabelStyle == null) {
                LabelStyle = new GUIStyle("label") {
                    alignment = TextAnchor.MiddleCenter,
                    normal = {textColor = TextColor},
                    fontSize = fontSize,
                };
            }

            if (ColorIconStyle == null) {
                ColorIconStyle = new GUIStyle("label") {
                    alignment = TextAnchor.MiddleCenter,
                };
            }
            if (CodeButtonStyle == null) {
                CodeButtonStyle = new GUIStyle("label") {
                    contentOffset = new Vector2(1, 1),
                    margin = new RectOffset(1, 1, 1, 1),
                    padding = new RectOffset(1, 1, 1, 1)
                };
            }

            if (circleIcon == null) {
                circleIcon = ResourceHolder.Instance.CircleIcon;
            }
            if (crossIcon == null) {
                crossIcon = ResourceHolder.Instance.CrossIcon;
            }
            if (copyIcon == null) {
                copyIcon = ResourceHolder.Instance.CopyImage;
            }
            if (pasteIcon == null) {
                pasteIcon = ResourceHolder.Instance.PasteImage;
            }
            if (listeners == null) {
                var presets = EditVal.useHDR ? DEFAULT_PRESET2 : DEFAULT_PRESET;
                listeners = new PresetListener<EditColorTex>[presets.Length+2];
                for (var i = 0; i < presets.Length; i++) {
                    var val = presets[i];
                    listeners[i] = new PresetListener<EditColorTex>(new GUIContent(val.ToString(CultureInfo.InvariantCulture)), ButtonStyle,
                        ect => {
                            var col = new Color(val, val, val, ect.Value.a);
                            ect.SetColor(ref col, true);
                        });
                }
                listeners[presets.Length] = new PresetListener<EditColorTex>(
                    new GUIContent("-"), ButtonStyle, ect => { ect.Add(-0.1f, true); });

                listeners[presets.Length+1] = new PresetListener<EditColorTex>(
                    new GUIContent("+"), ButtonStyle, ect => { ect.Add(0.1f, true); });
            }

            if (NumberUtil.Equals(rowHeight, 0, 0.1f)) {
                RowHeight = uiParamSet.itemHeight;
            }

            initialized = true;
        }

        /// <summary>
        /// x, y, widthは外から設定されることを想定
        /// heightは状況に応じて自動設定
        /// 
        /// </summary>
        /// <param name="uiParams">UIパラメータ</param>
        protected override void Layout(UIParamSet uiParams) {
            Margin = uiParams.margin;

            var leftPos = rect.x;
            if (text != null) {
                var labelWidth = LabelStyle.CalcSize(new GUIContent(text)).x;
                labelRect.Set(leftPos, rect.y, labelWidth, RowHeight);
                leftPos = labelRect.xMax + margin * 2;
            }
            colorIconRect.Set(leftPos, rect.y, EditVal.colorIconWidth, RowHeight);

            // 右詰めとして、描画時に右端から位置を算出
            presetRect.Set(0, rect.y, 0, RowHeight);
            
            var workPos = xMax;
            const int listenerMargin = 1;
            if (listeners != null) {
                for (var i=listeners.Length-1; i>=0; i--) {
                    var listener = listeners[i];
                    workPos -= listener.width + listenerMargin;
                }
            }

            // color-code
            var codeWidth = TextFStyle.CalcSize(new GUIContent("#DDDDDD")).x;
            workPos -= codeWidth + listenerMargin;
            colorCodeRect.Set(workPos, rect.y, codeWidth, RowHeight);

            // color-code icon
            workPos -= copyIcon.width + listenerMargin;
            colorCodeIcon2Rect.Set(workPos, rect.y, copyIcon.width, RowHeight);

            workPos -= copyIcon.width + listenerMargin;
            colorCodeIcon1Rect.Set(workPos, rect.y, copyIcon.width, RowHeight);
            
            // TODO 小さいrectが指定された場合:ScrollViewとして表示
            var xPos = rect.x + margin * 4;
            var yPos = rect.y + RowHeight + margin + GetMapMargin();
            var mapTex1 = EditVal.MapTex;
            mapTexRect.Set(xPos, yPos, mapTex1.width, mapTex1.height);

            xPos = mapTexRect.xMax + margin * 3;
            lightTexRect.Set(xPos, yPos, EditVal.lightTex.width, EditVal.lightTex.height);

            // カーソル位置は、posやlight値の影響を受けるため、描画時に位置は毎回調整する
            var iconTex = circleIcon;
            var offset = iconTex.width * 0.5f;
            mapCursorRect.Set(0, 0, iconTex.width, iconTex.width);
            lightCursorRect.Set(lightTexRect.x  + lightTexRect.width*0.5f - offset, 0, iconTex.width, iconTex.height);

            if (presetMgr != null) {
                presetRows = 0;
                var presetWidth = rect.width - (lightTexRect.xMax - rect.x + margin * 2);
                if (presetWidth > 0) {
                    var presetIcon = presetMgr.FocusIcon;
                    var iconWidth = presetIcon.width + presetIconMargin;
                    presetColumns = (int)presetWidth / iconWidth;
                    if (presetColumns > 0) {
                        var pbWidth = PresetButtonStyle.CalcSize(new GUIContent("Delete")).x;
                        var pbWidthMax = (presetWidth - presetIconMargin * 3) / 2f;
                        if (pbWidth > pbWidthMax) pbWidth = pbWidthMax;

                        yPos = yMax - uiParams.itemHeight;
                        presetButtonRect.Set(0, yPos, pbWidth, uiParams.itemHeight); // xPosは実行時に判断

                        var iconHeight = presetIcon.height + presetIconMargin;
                        presetRows = (int) (texHeight - uiParams.unitHeight - margin) / iconHeight;
                    }
                }
            }
        }

        protected virtual float GetMapMargin() {
            return 0f;
        }

        protected override void DrawGUI() {
            if (!initialized) return;

            if (Text != null) {
                if (GUI.Button(labelRect, Text, LabelStyle)) Expand = !expand;
            }
            if (GUI.Button(colorIconRect, EditVal.ColorTex, ColorIconStyle)) Expand = !expand;

            DrawColorCode();
            DrawPresetButtons();
            if (!expand) return;

            var offset = circleIcon.width * 0.5f;
            DrawMapTex(ref mapTexRect, offset);
            DrawLightTex(ref lightTexRect, offset);
            if (presetMgr != null) {
                var xPos = lightTexRect.xMax + margin * 3;
                var yPos = labelRect.yMax + margin * 2;
                DrawPreset(xPos, yPos);
            }
        }

        protected virtual void DrawColorCode() {
            // Color-Code
            if (GUI.Button(colorCodeIcon1Rect, copyIcon, CodeButtonStyle)) {
                GUIUtility.systemCopyBuffer = EditVal.ColorCode;
            }

            var clip = GUIUtility.systemCopyBuffer;
            enabledStore.SetEnabled(ColorUtils.IsColorCode(clip));
            try {
                if (GUI.Button(colorCodeIcon2Rect, pasteIcon, CodeButtonStyle)) {
                    EditVal.SetColorCode(clip);
                }
            } finally {
                enabledStore.Restore();
            }
            var editCode = GUI.TextField(colorCodeRect, EditVal.ColorCode, 7, TextFStyle);
            EditVal.SetColorCode(editCode);
        }

        protected virtual void DrawPresetButtons() {
            //
            // 右詰めとして、右端から位置を算出 (カラーコードは事前に位置を算出)
            //

            // (Slider) Preset
            if (listeners != null) {
                var xPos = xMax;
                const int listenerMargin = 1;
                for (var i=listeners.Length-1; i>=0; i--) {
                    var listener = listeners[i];
                    xPos -= listener.width + listenerMargin;
                    presetRect.x = xPos;
                    presetRect.width = listener.width;
                    if (GUI.Button(presetRect, listener.label, ButtonStyle)) {
                        listener.action(EditVal);
                    }
                }
            }
        }

        protected virtual void DrawMapTex(ref Rect mapRect, float offset) {
            GUI.DrawTexture(mapRect, EditVal.MapTex, ScaleMode.StretchToFill, true, 0f);

            mapCursorRect.x = mapRect.x + EditVal.pos.x - offset;
            mapCursorRect.y = mapRect.y + EditVal.pos.y - offset;
            GUI.DrawTexture(mapCursorRect, circleIcon, ScaleMode.StretchToFill, true, 0f);

            MapPickerEvent(ref mapRect);
        }

        protected virtual void DrawLightTex(ref Rect lightRect, float offset) {
            GUI.DrawTexture(lightRect, EditVal.lightTex, ScaleMode.StretchToFill, true, 0f);

            lightCursorRect.x = lightRect.x + lightTexRect.width * 0.5f - offset;
            lightCursorRect.y = lightRect.y + (EditVal.lightTex.height - 2 * lightBorderWidth) * (1 - EditVal.Light) - offset + lightBorderWidth;
            GUI.DrawTexture(lightCursorRect, circleIcon, ScaleMode.StretchToFill, true, 0f);

            LightSliderEvent(ref lightRect);
        }

        protected virtual void DrawPreset(float xPos, float yPos) {
            if (presetRows < 1) return;

            var e = Event.current;
            var mouseClicked = e.type == EventType.MouseDown && (e.button == 0 || e.button == 1);

            var icon = presetMgr.BaseIcon;
            var iconRect = new Rect(0, 0, icon.width, icon.height);
            var startIdx = 0;
            var columns = 0;
            var workPos = xPos;
            while (startIdx < presetMgr.Count) {
                var endIdx = (startIdx + presetRows < presetMgr.Count) ? startIdx + presetRows : presetMgr.Count;
                iconRect.x = workPos + presetIconMargin;
                iconRect.y = yPos - iconRect.height;
                for (var i = startIdx; i < endIdx; i++) {
                    iconRect.y += iconRect.height + presetIconMargin;
                    if (selectedPreset == i) {
                        GUI.Label(iconRect, presetMgr.FocusIcon, PresetIconStyle);
                    }
                    var presetIcon = presetMgr.presetIcons[i];
                    GUI.Label(iconRect, presetIcon, PresetIconStyle);

                    // プリセットの領域チェック
                    if (!mouseClicked) continue;
                    var mousePos = e.mousePosition;
                    if (iconRect.Contains(mousePos)) {
                        switch (e.button) {
                        case 1: // right click
                            presetMgr.ClearColor(selectedPreset);
                            selectedPreset = i;
                            break;
                        case 0: // left click
                            if (selectedPreset == i) {
                                var code = presetMgr.presetCodes[i];
                                EditVal.SetColorCode(code, true);
                            } else {
                                selectedPreset = i;
                            }
                            break;
                        }
                    }
                }

                workPos += iconRect.width + presetIconMargin;
                startIdx += presetRows;
                if (++columns >= presetColumns) break;
            }

            enabledStore.SetEnabled(selectedPreset != -1);
            try {
                presetButtonRect.x = xPos + presetIconMargin;
                if (GUI.Button(presetButtonRect, "Save", PresetButtonStyle)) {
                    var col = EditVal.Value;
                    presetMgr.SetColor(selectedPreset, EditVal.ColorCode, ref col);
                }

                presetButtonRect.x = presetButtonRect.xMax + presetIconMargin;
                if (GUI.Button(presetButtonRect, "Delete", PresetButtonStyle)) {
                    presetMgr.ClearColor(selectedPreset);
                }
            } finally {
                enabledStore.Restore();
            }
        }

        private void MapPickerEvent(ref Rect rect1) {
            if (lightDragging) return;

            var e = Event.current;
            if (e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)) {

                var mousePos = e.mousePosition;
                if (mapDragging || rect1.Contains(mousePos)) {
                    var x = (int) (mousePos.x - rect1.x);
                    var y = (int) (mousePos.y - rect1.y);

                    Color col;
                    if (mapDragging) {
                        var centerX = EditVal.MapTex.width / 2;
                        var centerY = Mathf.CeilToInt(EditVal.MapTex.height / 2f); // Y軸反転のため、奇数の場合は切り上げ
                        var radius = Math.Min(centerX, centerY);
                        var dist = ColorUtils.Distance(x, y, centerX, centerY);
                        if (dist <= radius) {
                            col = EditVal.GetMapColor(x, y);
                        } else {
                            // ドラッグ時は範囲を逸脱しても、角度を元に外周色に設定
                            col = ColorUtils.GetEdgeColor(x - centerX, -(y - centerY), dist) * EditVal.Light;
                            col.a = 1f;
                            var mul = radius / dist;
                            // 位置も外周に合わせて補正
                            x = (int)((x-centerX) * mul) + centerX;
                            y = (int)((y-centerY) * mul) + centerY;
                        }

                    } else if (e.type == EventType.MouseDown) {
                        col = EditVal.GetMapColor(x, y);
                        // 透過色の場合は無視
                        if (Equals(col.a, 0f)) return;
                        mapDragging = true;

                    } else {
                        return;
                    }
                    EditVal.pos.Set(x, y);
                    EditVal.Value = col;

                    e.Use();
                }
            } else if (mapDragging && e.type == EventType.MouseUp) {
                mapDragging = false;
            }
        }

        private void LightSliderEvent(ref Rect rect1) {
            if (mapDragging) return;

            var e = Event.current;
            if (e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)) {

                var mousePos = e.mousePosition;
                if (e.type == EventType.MouseDown && rect1.Contains(mousePos)) lightDragging = true;
                if (lightDragging) {
                    var light1 = 1f - (mousePos.y - rect1.y - lightBorderWidth)/(EditVal.lightTex.height - 2*lightBorderWidth);
                    if (1f < light1) light1 = 1f;
                    else if (light1 < 0f) light1 = 0f;

                    EditVal.Light = light1;
                    e.Use();
                }
            } else if (lightDragging && e.type == EventType.MouseUp) {
                lightDragging = false;
            }
        }
        #endregion

        #region Properties
        public override int FontSize {
            set {
                fontSize = value;
                if (LabelStyle != null) LabelStyle.fontSize = fontSize;
            }
        }

        protected int fontSizeS;
        public virtual int FontSizeS {
            get { return fontSizeS; }
            set {
                fontSizeS = value;
                if (TextFStyle != null) TextFStyle.fontSize = fontSizeS;
                if (ButtonStyle != null) ButtonStyle.fontSize = fontSizeS;
                if (PresetButtonStyle != null) PresetButtonStyle.fontSize = fontSizeS;
            }
        }
        public GUIStyle ButtonStyle { get; set; }
        // カラーアイコンスタイル
        public GUIStyle ColorIconStyle { get; set; }
        // ラベルスタイル
        public GUIStyle LabelStyle { get; set; }

        // テキストフィールドスタイル
        public GUIStyle TextFStyle { get; set; }
        // カラーコードボタンスタイル
        public GUIStyle CodeButtonStyle { get; set; }
        // カラープリセットアイコンスタイル
        public GUIStyle PresetIconStyle { get; set; }
        // カラープリセットボタンアイコンスタイル
        public GUIStyle PresetButtonStyle { get; set; }

        /// <summary>テキストラベル</summary>
        public override string Text {
            get { return text; }
            set { text = value; }
        }

        protected bool expand;
        public virtual bool Expand {
            get { return expand; }
            set {
                if (expand != value) {
                    expand = value;
                    UpdateHeight();

                    ExpandChanged(this, EventArgs.Empty);
                }
            }
        }
        protected float rowHeight;
        public float RowHeight {
            set {
                if (NumberUtil.Equals(rowHeight, value, 0.1f)) return;
                rowHeight = value;
                UpdateHeight();
            }
            get { return rowHeight; }
        }

        protected virtual void UpdateHeight() {
            var height = rowHeight;
            if (expand) height += margin +  texHeight;
            Height = height;
        }

        #endregion

        #region Fields
        public static readonly float[] DEFAULT_PRESET = {0, 0.5f, 1f};
        public static readonly float[] DEFAULT_PRESET2 = {0, 0.5f, 1f, 1.5f, 2f};
        public const int DEFAULT_BORDER_WIDTH = 1;

        public PresetListener<EditColorTex>[] listeners;

        //
        // 各種位置情報
        //
        protected Rect colorIconRect = new Rect();
        protected Rect labelRect = new Rect();
        protected Rect presetRect;
        protected Rect colorCodeRect = new Rect();
        protected Rect colorCodeIcon1Rect = new Rect();
        protected Rect colorCodeIcon2Rect = new Rect();
        protected Rect mapTexRect;
        protected Rect lightTexRect;
        protected Rect mapCursorRect;
        protected Rect lightCursorRect;
        protected Rect presetButtonRect;

        // 外部から指定する必要のあるUIサイズ
        /// <summary>プリセットカラーアイコンのマージン</summary>
        public int presetIconMargin = 2;

        /// <summary>
        /// 輝度テクスチャの縁サイズ.
        /// コンストラクタに指定したlightTexの縁の太さがデフォルト以外の場合は、これを変更する必要がある.
        /// </summary>
        public int lightBorderWidth = DEFAULT_BORDER_WIDTH;

        // work value
        protected bool initialized;
        protected int presetRows;
        protected int presetColumns;
        private int selectedPreset = -1;
        private bool mapDragging;
        private bool lightDragging;
        protected readonly int texHeight;

        // アイコンリソース
        public Texture2D circleIcon;
        public Texture2D crossIcon;
        public Texture2D copyIcon;
        public Texture2D pasteIcon;

        protected readonly ColorPresetManager presetMgr;
        public EditColorTex EditVal { get; protected set; }

        #endregion

        #region Event
        public event EventHandler ExpandChanged = delegate { };

        #endregion
    }
}
