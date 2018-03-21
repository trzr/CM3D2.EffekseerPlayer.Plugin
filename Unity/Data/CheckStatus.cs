namespace EffekseerPlayerPlugin.Unity.Data {
    /// <summary>
    /// チェックステータスを表す列挙型.
    /// </summary>
    public enum CheckStatus {
        /// <summary>チェック無し</summary>
        NoChecked = 0,
        /// <summary>チェック済み</summary>
        Checked = 1,
        /// <summary>一部チェック</summary>
        PartChecked = 2,
    }
}