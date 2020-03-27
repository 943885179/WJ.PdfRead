using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WJ.PdfRead.Model
{
    public class PDFResponse
    {
        public ResultData data { get; set; }
        public int code { get; set; }
        public string  msg { get; set; }
    }
}
