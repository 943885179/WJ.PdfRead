using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WJ.PdfRead.Model
{
    /// <summary>
    /// 基本信息
    /// </summary>
    public class BasicInformation
    {
        /// <summary>
        /// 报告编号
        /// </summary>
        public string Bgbh { get; set; }
        /// <summary>
        /// 查询时间
        /// </summary>
        public string SeachTime { get; set; }
        /// <summary>
        /// 报告时间
        /// </summary>
        public string BGTime { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        public string CardType { get; set; }
        /// <summary>
        /// 证件号码
        /// </summary>
        public string IdCard { get; set; }
        /// <summary>
        /// 婚姻状况
        /// </summary>
        public string HY { get; set; }
    }
}
