using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WJ.PdfRead.Model
{
    public class ResultData
    {
        /// <summary>
        /// 基本信息
        /// </summary>
        public BasicInformation BasicInformation { get; set; }
        /// <summary>
        /// 保证人资产处置信息
        /// </summary>
        public SummaryInformation ZcckBzrdc { get; set; }
        /// <summary>
        /// 公共记录
        /// </summary>
        public Common Common { get; set; }
        /// <summary>
        /// 交易记录
        /// </summary>
        public SummaryInformation SummaryInformation { get; set; }
        /// <summary>
        /// 查询记录
        /// </summary>
        public SeachList SeachList { get; set; }

    }
}
