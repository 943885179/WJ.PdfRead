using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WJ.PdfRead.Model
{
    public class SeachList
    {
        public List<Seach> Seachs { get; set; }
    }
    /// <summary>
    /// 查询记录
    /// </summary>
    public class Seach
    {
        /// <summary>
        /// 内容
        /// </summary>
        public List<SearchContent> SearchContents { get; set; }
        /// <summary>
        /// 机构还是个人查询
        /// </summary>
        public SearchManType SearchType { get; set; }
        /// <summary>
        /// 中文
        /// </summary>
        public string SearchType_CN { get; set; }
    }
    /// <summary>
    /// 查询内容
    /// </summary>
    public class SearchContent
    {
        public string Id { get; set; }
        public string Time { get; set; }
        public string CZY { get; set; }
        public string YY { get; set; }
    }
    /// <summary>
    /// 行
    /// </summary>
    public class SearchRow
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string Id { get; set; }
    }
    /// <summary>
    ///表头
    /// </summary>
    public class SearchTitle
    {

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public SearchTitleEnum Type { get; set; }
    }
    /// <summary>
    /// 查询类型
    /// </summary>
    public enum SearchManType
    {
        /// <summary>
        /// 机构
        /// </summary>
        [Description("机构")]
        JG,
        /// <summary>
        /// 个人
        /// </summary>
        [Description("个人")]
        ONE
    }
    /// <summary>
    /// 查询行数
    /// </summary>
    public enum SearchTitleEnum
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Description("编号")]
        BH,
        /// <summary>
        /// 查询时间
        /// </summary>
        [Description("查询时间")]
        TIME,
        /// <summary>
        /// 查询操作员
        /// </summary>
        [Description("查询操作员")]
        CZY,
        /// <summary>
        /// 查询原因
        /// </summary>
        [Description("查询原因")]
        YY

    }
}
