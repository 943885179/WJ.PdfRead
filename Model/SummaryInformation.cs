using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace WJ.PdfRead.Model
{
    /// <summary>
    /// 交易记录
    /// </summary>
    public class SummaryInformation
    {
        /// <summary>
        /// 贷款
        /// </summary>
        public List<DK> DKs { get; set; }
    }

    /// <summary>
    /// 贷款
    /// </summary>
    public class DK
    {
        /// <summary>
        /// 贷款类型
        /// </summary>
        public DKLXENUM DKLX { get; set; }
        public string DKLX_CN { get; set; }
        /// <summary>
        /// 账户数
        /// </summary>
        public string ZH { get; set; }
        /// <summary>
        /// 未结清、未销户账户数
        /// </summary>
        public string WJQ { get; set; }
        /// <summary>
        /// 发生过逾期的账户数
        /// </summary>
        public string YQ { get; set; }
        /// <summary>
        ///  发生过90天以上逾期的账户数
        /// </summary>
        public string YQ90 { get; set; }
        /// <summary>
        /// 为他人担保笔数
        /// </summary>
        public string DB { get; set; }
        /// <summary>
        /// 笔数
        /// </summary>
        public string BS { get; set; }

        /// <summary>
        /// 逾期记录 发生过逾期的贷记卡账户明细如下：
        /// </summary>
        public List<Record> BadRecord { get; set; }
        /// <summary>
        /// 正常记录 从未逾期过的贷记卡及透支未超过60天的准贷记卡账户明细如下 从未逾期过的账户明细如下
        /// </summary>
        public List<Record> GoodRecord { get; set; }
        /// <summary>
        /// 为他人担保信息
        /// </summary>
        public List<Record> DBRecord { get; set; }

    }
    /// <summary>
    /// 明细列表
    /// </summary>
    public class Record {
        /// <summary>
        /// 编号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 内容描述
        /// </summary>
        public string value { get; set; }
        public string title { get; set; }
    }

    /// <summary>
    /// 行
    /// </summary>
    public class RecordRow
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string Id { get; set; }
    }
    /// <summary>
    /// 贷款类型 信用卡 购房贷款 其他贷款
    /// </summary>
    public class DKLXDTO
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public DKLXENUM Type { get; set; }
    }
    /// <summary>
    /// 贷款状态
    /// </summary>
    public class DKZTDTO
    {
        public float X { get; set; }
        public float Y { get; set; }
        public DKZTENUM Type { get; set; }
    }
    public enum DKZTENUM
    {
        /// <summary>
        /// 账户数
        /// </summary>
        [Description("账户数")]
        ZH,
        /// <summary>
        /// 未结清、未销户账户数
        /// </summary>
        [Description("未销户账户数")]
        WJQ,
        /// <summary>
        /// 发生过逾期的账户数
        /// </summary>
        [Description("发生过逾期的账户数")]
        YQ,
        /// <summary>
        ///  发生过90天以上逾期的账户数
        /// </summary>
        [Description("发生过90天以上逾期的账户数")]
        YQ90,
        /// <summary>
        /// 为他人担保笔数
        /// </summary>
        [Description("为他人担保笔数")]
        DB,
        /// <summary>
        /// 为他人担保笔数
        /// </summary>
        [Description("笔数")]
        BS

    }
    public enum DKLXENUM
    {
        /// <summary>
        /// 信用卡
        /// </summary>
        [Description("信用卡")]
        XYK,
        /// <summary>
        /// 购房贷款
        /// </summary>
        [Description("购房贷款")]
        GFDK,
        /// <summary>
        /// 其他贷款
        /// </summary>
        [Description("其他贷款")]
        QTDK,
        /// <summary>
        /// 账户数
        /// </summary>
        [Description("资产处置信息")]
        ZCCZ,
        /// <summary>
        /// 未结清、未销户账户数
        /// </summary>
        [Description("保证人代偿信息")]
        BZRDC,
    }


}
