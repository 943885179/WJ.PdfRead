using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spire.Pdf;
using WJ.PdfRead.comman;
using WJ.PdfRead.Model;

namespace WJ.PdfRead.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        /// <summary>
        /// 获取PDF解析文件通过地址（本地）
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [HttpGet(@"ReadByFilePath/{filePath=G:\陈贵年 2019.08.06个人信用报告.pdf}")]
        public string ReadByFilePath(string filePath = @"G:\yy微信支付交易明细证明.pdf")
        {
            filePath = @"G:\yy微信支付交易明细证明.pdf";
            //创建PdfDocument实例
            PdfDocument pdf = new PdfDocument();
            //加载PDF文档
            pdf.LoadFromFile(filePath);
            var sb = new StringBuilder();
            var tt = "";
            foreach (PdfPageBase page in pdf.Pages)
            {
                string text = page.ExtractText(new RectangleF(0, 0, 1000, 1000));
                tt += text;
                var spStr = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                //清除第一和最后一个：无效
                var spStrs = spStr.Skip(1).Take(spStr.Count() - 2).ToList();
                foreach (var sp in spStrs)
                {
                    sb.Append(sp);
                }
                /*
                 Image[] ss = page.ExtractImages();
                   Bitmap images = new Bitmap(ss[0]);
                   images.Save("G:\\name.png");*/
            }

            var x = sb.ToString();
            return tt;
        }
        /// <summary>
        /// 上传pdf获取pdf内容
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("ReadByFile")]
        public string ReadByFile(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return "请上传文件";
                }
                if (Path.GetExtension(file.FileName).ToUpper() != ".PDF")
                {
                    return "文件不是pdf文件";
                }
                var dir = "pdf";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var filePath = Path.Combine(dir, file.FileName);
                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    PdfDocument pdf = new PdfDocument();
                    pdf.LoadFromStream(fs);
                    var sb = new StringBuilder();
                    foreach (PdfPageBase page in pdf.Pages)
                    {
                        string text = page.ExtractText(new RectangleF(0, 0, 1000, 1000));
                        var spStr = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        //清除第一和最后一个：无效
                        var spStrs = spStr.Skip(1).Take(spStr.Count() - 2).ToList();
                        foreach (var sp in spStrs)
                        {
                            sb.Append(sp);
                        }
                    }
                    fs.Flush();
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                return "读取文件错误！" + ex.ToString();
            }
        }

        /// <summary>
        /// 上传简版征信pdf获取pdf内容
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("JBReadByFile")]
        public PDFResponse JBReadByFile(IFormFile file)
        {
            var response = new PDFResponse();
            try
            {
                if (file == null)
                {
                    response.code = -1;
                    response.msg = "请上传文件";
                    return response;
                }
                if (Path.GetExtension(file.FileName).ToUpper() != ".PDF")
                {
                    response.code = -1;
                    response.msg = "文件不是pdf文件";
                    return response;
                }
                var dir = "pdf";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                var filePath = Path.Combine(dir, file.FileName);
                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    file.CopyTo(fs);
                    fs.Flush();
                    PdfDocument pdf = new PdfDocument();
                    pdf.LoadFromStream(fs);
                    var result = new ResultData()
                    {
                        BasicInformation = new BasicInformation(),
                        Common = new Common()
                        {
                            str = new List<string>()
                        },
                        SeachList = new SeachList()
                        {
                            Seachs = new List<Seach>()
                        },
                        SummaryInformation = new SummaryInformation()
                        {
                            DKs = new List<DK>(),
                        },
                    };
                    bool zcczRead = true;//读取资产处置信息关键字
                    bool bzrdcRead = true;//读取保证人贷偿关键字
                    bool xykRead = true;//读取信用卡关键字
                    bool gfdkRead = true;//读取购房贷款关键字
                    bool qtdkRead = true;//读取其他贷款关键字
                    bool isIntoTab = false;//表格内容是否开始读取
                    bool zcczTitle = false;//资产处置记录
                    bool bzrdcTitle = false;//保证人代偿记录
                    bool xykTitle = false;//信用卡记录
                    bool gfdkTitle = false;//购房贷款记录
                    bool qtdkTitle = false;//其他贷款记录
                    bool isBadRecord = false;//是否记录逾期数据
                    bool isGoodRecord = false;//是否记录正常数据
                    bool isComman = false;//是否公共记录
                    bool isSeachJG = false;//是否查询记录（机构)
                    bool isSeachONE = false;//是否查询记录（个人）
                    var DKLXDTOs = new List<DKLXDTO>();//临时保存贷款类型；
                    var DKZTDTOs = new List<DKZTDTO>();//临时保存贷款状态
                    var searchTitles = new List<SearchTitle>();//临时保存查询记录表头
                    var searchRows = new List<SearchRow>();//临时保存查询记录行
                    var recordRows = new List<RecordRow>();//临时保存贷款明细
                    var lxcn = "";
                    #region //解析pdf
                    foreach (PdfPageBase page in pdf.Pages)
                    {
                        var allTxt = page.FindAllText();
                        foreach (var txt in allTxt.Finds)
                        {
                            var trimTxt = txt.SearchText.Trim();
                            Regex reg = new Regex(@"(第|共)\d{1,}页");
                            if (string.IsNullOrEmpty(trimTxt))
                            {//页码和空白直接过滤

                            }
                            else if (reg.IsMatch(trimTxt))
                            {
                                DKLXDTOs = new List<DKLXDTO>();//临时保存贷款类型；
                                DKZTDTOs = new List<DKZTDTO>();//临时保存贷款状态
                                searchTitles = new List<SearchTitle>();//临时保存查询记录表头
                                searchRows = new List<SearchRow>();//临时保存查询记录行
                                recordRows = new List<RecordRow>();//临时保存贷款明细
                            }
                            #region //基本信息
                            else if (trimTxt.StartsWith("报告编号:"))
                            {
                                result.BasicInformation.Bgbh = trimTxt.Replace("报告编号:", "");
                            }
                            else if (trimTxt.StartsWith("查询时间:"))
                            {
                                result.BasicInformation.SeachTime = trimTxt.Replace("查询时间: ", "");
                            }
                            else if (trimTxt.StartsWith("报告时间:"))
                            {
                                result.BasicInformation.BGTime = trimTxt.Replace("报告时间: ", "");
                            }
                            else if (trimTxt.StartsWith("姓名:"))
                            {
                                result.BasicInformation.Name = trimTxt.Replace("姓名: ", "");
                            }
                            else if (trimTxt.StartsWith("证件类型:"))
                            {
                                result.BasicInformation.CardType = trimTxt.Replace("证件类型: ", "");
                            }
                            else if (trimTxt.StartsWith("证件号码:"))
                            {
                                result.BasicInformation.IdCard = trimTxt.Replace("证件号码: ", "");
                            }
                            else if (trimTxt.Equals("未婚") || trimTxt.Equals("已婚"))
                            {
                                result.BasicInformation.HY = trimTxt;
                            }
                            #endregion
                            #region //资产处置信息
                            else if (trimTxt.Equals("资产处置信息") && zcczRead)
                            {
                                isIntoTab = true;
                                zcczRead = !zcczRead;
                                DKLXDTOs.Add(new DKLXDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Width = txt.Bounds.Width,
                                    Type = DKLXENUM.ZCCZ
                                });
                            }
                            else if (trimTxt.Equals("保证人代偿信息") && bzrdcRead)
                            {
                                bzrdcRead = !bzrdcRead;
                                DKLXDTOs.Add(new DKLXDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Width = txt.Bounds.Width,
                                    Type = DKLXENUM.BZRDC
                                });
                            }
                            #endregion
                            #region //贷款类型给坐标
                            else if (trimTxt.Equals("信用卡") && xykRead)
                            {
                                isIntoTab = true;
                                xykRead = !xykRead;
                                DKLXDTOs.Add(new DKLXDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Width = txt.Bounds.Width,
                                    Type = DKLXENUM.XYK
                                });
                            }
                            else if (trimTxt.Equals("购房贷款") && gfdkRead)
                            {
                                gfdkRead = !gfdkRead;
                                DKLXDTOs.Add(new DKLXDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Width = txt.Bounds.Width,
                                    Type = DKLXENUM.GFDK
                                });
                            }
                            else if (trimTxt.Equals("其他贷款") && qtdkRead)
                            {
                                qtdkRead = !qtdkRead;
                                DKLXDTOs.Add(new DKLXDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Width = txt.Bounds.Width,
                                    Type = DKLXENUM.QTDK
                                });
                            }
                            #endregion
                            #region //贷款状态记录坐标
                            else if (trimTxt.Equals("笔数"))
                            {
                                DKZTDTOs.Add(new DKZTDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Type = DKZTENUM.BS,
                                });
                            }
                            else if (trimTxt.Equals("账户数"))
                            {
                                DKZTDTOs.Add(new DKZTDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Type = DKZTENUM.ZH,
                                });
                            }
                            else if (trimTxt.Equals("未结清/未销户账户数"))
                            {
                                DKZTDTOs.Add(new DKZTDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Type = DKZTENUM.WJQ,
                                });
                            }
                            else if (trimTxt.Equals("发生过逾期的账户数"))
                            {
                                DKZTDTOs.Add(new DKZTDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Type = DKZTENUM.YQ,
                                });
                            }
                            else if (trimTxt.Equals("发生过90天以上逾期的账户数"))
                            {
                                DKZTDTOs.Add(new DKZTDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Type = DKZTENUM.YQ90,
                                });
                            }
                            else if (trimTxt.Equals("为他人担保笔数"))
                            {
                                DKZTDTOs.Add(new DKZTDTO()
                                {
                                    X = txt.Bounds.X,
                                    Y = txt.Bounds.Y,
                                    Type = DKZTENUM.DB
                                });
                            }
                            #endregion
                            #region // 贷款表格内容填充
                            else if (DKLXDTOs.Any(o => o.X <= txt.Bounds.X && txt.Bounds.X <= o.X + o.Width) && DKZTDTOs.Any(o => o.Y == txt.Bounds.Y) && isIntoTab)
                            {
                                var lx = DKLXDTOs.Where(o => o.X <= txt.Bounds.X && txt.Bounds.X <= o.X + o.Width).OrderByDescending(o => o.Y).Select(o => o.Type).First();
                                var zt = DKZTDTOs.Where(o => o.Y == txt.Bounds.Y).Select(o => o.Type).First();

                                if (!result.SummaryInformation.DKs.Any(o => o.DKLX == lx))
                                {
                                    result.SummaryInformation.DKs.Add(new DK
                                    {
                                        DKLX = lx,
                                        DKLX_CN = EnumUtil.GetDescription(lx),
                                        BadRecord = new List<Record>(),
                                        GoodRecord = new List<Record>()
                                    });
                                }
                                var dk = result.SummaryInformation.DKs.First(o => o.DKLX == lx);
                                switch (lx)
                                {
                                    case DKLXENUM.XYK:
                                        switch (zt)
                                        {
                                            case DKZTENUM.ZH:
                                                dk.ZH = trimTxt;
                                                break;
                                            case DKZTENUM.WJQ:
                                                dk.WJQ = trimTxt;
                                                break;
                                            case DKZTENUM.YQ:
                                                dk.YQ = trimTxt;
                                                break;
                                            case DKZTENUM.YQ90:
                                                dk.YQ90 = trimTxt;
                                                break;
                                            case DKZTENUM.DB:
                                                dk.DB = trimTxt;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case DKLXENUM.GFDK:
                                        switch (zt)
                                        {
                                            case DKZTENUM.ZH:
                                                dk.ZH = trimTxt;
                                                break;
                                            case DKZTENUM.WJQ:
                                                dk.WJQ = trimTxt;
                                                break;
                                            case DKZTENUM.YQ:
                                                dk.YQ = trimTxt;
                                                break;
                                            case DKZTENUM.YQ90:
                                                dk.YQ90 = trimTxt;
                                                break;
                                            case DKZTENUM.DB:
                                                dk.DB = trimTxt;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case DKLXENUM.QTDK:
                                        switch (zt)
                                        {
                                            case DKZTENUM.ZH:
                                                dk.ZH = trimTxt;
                                                break;
                                            case DKZTENUM.WJQ:
                                                dk.WJQ = trimTxt;
                                                break;
                                            case DKZTENUM.YQ:
                                                dk.YQ = trimTxt;
                                                break;
                                            case DKZTENUM.YQ90:
                                                dk.YQ90 = trimTxt;
                                                break;
                                            case DKZTENUM.DB:
                                                dk.DB = trimTxt;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case DKLXENUM.ZCCZ:
                                        switch (zt)
                                        {
                                            case DKZTENUM.BS:
                                                dk.BS = trimTxt;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case DKLXENUM.BZRDC:
                                        switch (zt)
                                        {
                                            case DKZTENUM.BS:
                                                dk.BS = trimTxt;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            #endregion
                            else if (trimTxt.Equals("资产处置信息") && !zcczRead)
                            {
                                isIntoTab = false;
                                zcczTitle = true;
                                xykTitle = false;
                                isBadRecord = true;
                                isGoodRecord = false;
                                lxcn = "资产处置信息";
                            }
                            else if (trimTxt.Equals("保证人代偿信息") && !bzrdcRead)
                            {
                                isIntoTab = false;
                                zcczTitle = false;
                                bzrdcTitle = true;
                                xykTitle = false;
                                isBadRecord = true;
                                isGoodRecord = false;
                                lxcn = "保证人代偿信息";
                            }
                            else if (trimTxt.Equals("信用卡") && !xykRead)
                            {
                                isIntoTab = false;
                                xykTitle = true;
                                zcczTitle = false;
                                bzrdcTitle = false;
                            }
                            else if (trimTxt.Equals("购房贷款") && !gfdkRead)
                            {
                                xykTitle = false;
                                gfdkTitle = true;
                            }
                            else if (trimTxt.Equals("其他贷款") && !gfdkRead)
                            {
                                xykTitle = false;
                                gfdkTitle = false;
                                qtdkTitle = true;
                            }
                            else if (trimTxt.Equals("公共记录"))
                            {
                                xykTitle = false;//信用卡记录
                                gfdkTitle = false;//信用卡记录
                                qtdkTitle = false;//信用卡记录
                                isBadRecord = false;//是否记录逾期数据
                                isGoodRecord = false;//是否记录正常数据
                                isComman = true;
                                result.Common.str = new List<string>();
                            }
                            else if (trimTxt.Equals("查询记录"))
                            {
                                isComman = false;
                            }
                            else if (trimTxt.Equals("说  明"))
                            {
                                isSeachJG = false;
                                isSeachONE = false;

                            }
                            else if (trimTxt.Equals("资产处理信息") || trimTxt.Equals("保证人代偿信息") || trimTxt.Equals("为他人担保信息") || trimTxt.Equals("发生过逾期的贷记卡账户明细如下：") || trimTxt.Equals("发生过逾期的账户明细如下："))
                            {//开始记录逾期记录
                                isBadRecord = true;
                                isGoodRecord = false;
                                lxcn = trimTxt;
                            }
                            else if (trimTxt.Equals("从未逾期过的贷记卡及透支未超过60天的准贷记卡账户明细如下：") || trimTxt.Equals("从未逾期过的账户明细如下："))
                            {//开始记录正常记录
                                isBadRecord = false;
                                isGoodRecord = true;
                                lxcn = trimTxt;
                            }
                            else if (trimTxt.Equals("机构查询记录明细"))
                            {
                                isSeachJG = true;
                                if (!result.SeachList.Seachs.Any(o => o.SearchType == SearchManType.JG))
                                {
                                    result.SeachList.Seachs.Add(new Seach()
                                    {
                                        SearchContents = new List<SearchContent>(),
                                        SearchType = SearchManType.JG,
                                        SearchType_CN = EnumUtil.GetDescription(SearchManType.JG)
                                    });
                                }
                            }
                            else if (trimTxt.Equals("本人查询记录明细"))
                            {
                                isSeachONE = true;
                                isSeachJG = false;
                                if (!result.SeachList.Seachs.Any(o => o.SearchType == SearchManType.ONE))
                                {
                                    result.SeachList.Seachs.Add(new Seach()
                                    {
                                        SearchContents = new List<SearchContent>(),
                                        SearchType = SearchManType.ONE,
                                        SearchType_CN = EnumUtil.GetDescription(SearchManType.ONE)
                                    });
                                }
                            }
                            else if (isBadRecord)
                            {
                                if (zcczTitle)
                                {
                                    var jl = result.SummaryInformation.DKs.FirstOrDefault(o => o.DKLX == DKLXENUM.ZCCZ);
                                    var isBhReg = new Regex(@"^\d{1,}\.");
                                    if (trimTxt.EndsWith('.') && isBhReg.IsMatch(trimTxt))
                                    {//是编号
                                        jl.BadRecord.Add(new Record()
                                        {
                                            Id = trimTxt.Replace(".", ""),
                                            title = lxcn

                                        });
                                        recordRows.Add(new RecordRow()
                                        {
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Id = trimTxt.Replace(".", "")
                                        });
                                    }
                                    else
                                    {
                                        var row = new RecordRow();
                                        int rowIndex = 0;
                                        while (rowIndex <= 2)
                                        {
                                            rowIndex++;
                                            row = recordRows.FirstOrDefault(r => r.Y - (8 * rowIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * rowIndex));
                                            if (row != null)
                                            {
                                                break;
                                            }
                                        }
                                        var yq = jl.BadRecord.FirstOrDefault(y => y.Id == row.Id);
                                        yq.value += trimTxt;
                                    }
                                }
                                else if (bzrdcTitle)
                                {
                                    var jl = result.SummaryInformation.DKs.FirstOrDefault(o => o.DKLX == DKLXENUM.BZRDC);
                                    var isBhReg = new Regex(@"^\d{1,}\.");
                                    if (trimTxt.EndsWith('.') && isBhReg.IsMatch(trimTxt))
                                    {//是编号
                                        jl.BadRecord.Add(new Record()
                                        {
                                            Id = trimTxt.Replace(".", ""),
                                            title = lxcn
                                        });
                                        recordRows.Add(new RecordRow()
                                        {
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Id = trimTxt.Replace(".", "")
                                        });
                                    }
                                    else
                                    {
                                        var row = new RecordRow();
                                        int rowIndex = 0;
                                        while (rowIndex <= 2)
                                        {
                                            rowIndex++;
                                            row = recordRows.FirstOrDefault(r => r.Y - (8 * rowIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * rowIndex));
                                            if (row != null)
                                            {
                                                break;
                                            }
                                        }
                                        var yq = jl.BadRecord.FirstOrDefault(y => y.Id == row.Id);
                                        yq.value += trimTxt;
                                    }
                                }
                                else if (xykTitle)
                                {
                                    var jl = result.SummaryInformation.DKs.FirstOrDefault(o => o.DKLX == DKLXENUM.XYK);
                                    var isBhReg = new Regex(@"^\d{1,}\.");
                                    if (trimTxt.EndsWith('.') && isBhReg.IsMatch(trimTxt))
                                    {//是编号
                                        jl.BadRecord.Add(new Record()
                                        {
                                            Id = trimTxt.Replace(".", ""),
                                            title = lxcn
                                        });
                                        recordRows.Add(new RecordRow()
                                        {
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Id = trimTxt.Replace(".", "")
                                        });
                                    }
                                    else
                                    {
                                        var row = new RecordRow();
                                        int rowIndex = 0;
                                        while (rowIndex <= 2)
                                        {
                                            rowIndex++;
                                            row = recordRows.FirstOrDefault(r => r.Y - (8 * rowIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * rowIndex));
                                            if (row != null)
                                            {
                                                break;
                                            }
                                        }
                                        var yq = jl.BadRecord.FirstOrDefault(y => y.Id == row.Id);
                                        yq.value += trimTxt;
                                    }
                                }
                                else if (gfdkTitle)
                                {
                                    var jl = result.SummaryInformation.DKs.FirstOrDefault(o => o.DKLX == DKLXENUM.GFDK);
                                    var isBhReg = new Regex(@"^\d{1,}\.");
                                    if (trimTxt.EndsWith('.') && isBhReg.IsMatch(trimTxt))
                                    {//是编号
                                        jl.BadRecord.Add(new Record()
                                        {
                                            Id = trimTxt.Replace(".", ""),
                                            title = lxcn
                                        });
                                        recordRows.Add(new RecordRow()
                                        {
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Id = trimTxt.Replace(".", "")
                                        });
                                    }
                                    else
                                    {
                                        var row = new RecordRow();
                                        int rowIndex = 0;
                                        while (rowIndex <= 2)
                                        {
                                            rowIndex++;
                                            row = recordRows.FirstOrDefault(r => r.Y - (8 * rowIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * rowIndex));
                                            if (row != null)
                                            {
                                                break;
                                            }
                                        }
                                        var yq = jl.BadRecord.FirstOrDefault(y => y.Id == row.Id);
                                        yq.value += trimTxt;
                                    }
                                }
                                else if (qtdkTitle)
                                {
                                    var jl = result.SummaryInformation.DKs.FirstOrDefault(o => o.DKLX == DKLXENUM.QTDK);
                                    var isBhReg = new Regex(@"^\d{1,}\.");
                                    if (trimTxt.EndsWith('.') && isBhReg.IsMatch(trimTxt))
                                    {//是编号
                                        jl.BadRecord.Add(new Record()
                                        {
                                            Id = trimTxt.Replace(".", ""),
                                            title = lxcn
                                        });
                                        recordRows.Add(new RecordRow()
                                        {
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Id = trimTxt.Replace(".", "")
                                        });
                                    }
                                    else
                                    {
                                        var row = new RecordRow();
                                        int rowIndex = 0;
                                        while (rowIndex <= 2)
                                        {
                                            rowIndex++;
                                            row = recordRows.FirstOrDefault(r => r.Y - (8 * rowIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * rowIndex));
                                            if (row != null)
                                            {
                                                break;
                                            }
                                        }
                                        var yq = jl.BadRecord.FirstOrDefault(y => y.Id == row.Id);
                                        yq.value += trimTxt;
                                    }
                                }
                                else
                                {

                                }
                            }
                            else if (isGoodRecord)
                            {
                                if (xykTitle)
                                {
                                    var jl = result.SummaryInformation.DKs.FirstOrDefault(o => o.DKLX == DKLXENUM.XYK);
                                    var isBhReg = new Regex(@"^\d{1,}\.");
                                    if (trimTxt.EndsWith('.') && isBhReg.IsMatch(trimTxt))
                                    {//是编号
                                        jl.GoodRecord.Add(new Record()
                                        {
                                            Id = trimTxt.Replace(".", ""),
                                            title = lxcn
                                        });
                                        recordRows.Add(new RecordRow()
                                        {
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Id = trimTxt.Replace(".", "")
                                        });
                                    }
                                    else
                                    {
                                        var row = new RecordRow();
                                        int rowIndex = 0;
                                        while (rowIndex <= 2)
                                        {
                                            rowIndex++;
                                            row = recordRows.FirstOrDefault(r => r.Y - (8 * rowIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * rowIndex));
                                            if (row != null)
                                            {
                                                break;
                                            }
                                        }
                                        var good = jl.GoodRecord.FirstOrDefault(y => y.Id == row.Id);
                                        good.value += trimTxt;
                                    }
                                }
                                else if (gfdkTitle)
                                {
                                    var jl = result.SummaryInformation.DKs.FirstOrDefault(o => o.DKLX == DKLXENUM.GFDK);
                                    var isBhReg = new Regex(@"^\d{1,}\.");
                                    if (trimTxt.EndsWith('.') && isBhReg.IsMatch(trimTxt))
                                    {//是编号
                                        jl.GoodRecord.Add(new Record()
                                        {
                                            Id = trimTxt.Replace(".", ""),
                                            title = lxcn
                                        });
                                        recordRows.Add(new RecordRow()
                                        {
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Id = trimTxt.Replace(".", "")
                                        });
                                    }
                                    else
                                    {
                                        var row = new RecordRow();
                                        int rowIndex = 0;
                                        while (rowIndex <= 2)
                                        {
                                            rowIndex++;
                                            row = recordRows.FirstOrDefault(r => r.Y - (8 * rowIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * rowIndex));
                                            if (row != null)
                                            {
                                                break;
                                            }
                                        }
                                        var good = jl.GoodRecord.FirstOrDefault(y => y.Id == row.Id);
                                        good.value += trimTxt;
                                    }
                                }
                                else if (qtdkTitle)
                                {
                                    var jl = result.SummaryInformation.DKs.FirstOrDefault(o => o.DKLX == DKLXENUM.QTDK);
                                    var isBhReg = new Regex(@"^\d{1,}\.");
                                    if (trimTxt.EndsWith('.') && isBhReg.IsMatch(trimTxt))
                                    {//是编号
                                        jl.GoodRecord.Add(new Record()
                                        {
                                            Id = trimTxt.Replace(".", ""),
                                            title = lxcn
                                        });
                                        recordRows.Add(new RecordRow()
                                        {
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Id = trimTxt.Replace(".", "")
                                        });
                                    }
                                    else
                                    {
                                        var row = new RecordRow();
                                        int rowIndex = 0;
                                        while (rowIndex <= 2)
                                        {
                                            rowIndex++;
                                            row = recordRows.FirstOrDefault(r => r.Y - (8 * rowIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * rowIndex));
                                            if (row != null)
                                            {
                                                break;
                                            }
                                        }
                                        var good = jl.GoodRecord.FirstOrDefault(y => y.Id == row.Id);
                                        good.value += trimTxt;
                                    }
                                }
                                else
                                {

                                }
                            }
                            else if (isComman)
                            {
                                result.Common.str.Add(txt.SearchText);
                            }
                            else if (isSeachJG)
                            {
                                var search = result.SeachList.Seachs.First(o => o.SearchType == SearchManType.JG);
                                switch (trimTxt)
                                {
                                    case "编号":
                                        searchTitles = new List<SearchTitle>();
                                        searchTitles.Add(new SearchTitle()
                                        {
                                            Type = SearchTitleEnum.BH,
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Width = txt.Bounds.Width
                                        });
                                        break;
                                    case "查询日期":
                                        searchTitles.Add(new SearchTitle()
                                        {
                                            Type = SearchTitleEnum.TIME,
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Width = txt.Bounds.Width
                                        });
                                        break;
                                    case "查询操作员":
                                        searchTitles.Add(new SearchTitle()
                                        {
                                            Type = SearchTitleEnum.CZY,
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Width = txt.Bounds.Width
                                        });
                                        break;
                                    case "查询原因":
                                        searchTitles.Add(new SearchTitle()
                                        {
                                            Type = SearchTitleEnum.YY,
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Width = txt.Bounds.Width
                                        });
                                        break;
                                    default:
                                        var title = searchTitles.FirstOrDefault(o => (o.X <= txt.Bounds.X && txt.Bounds.X <= o.X + o.Width) || (txt.Bounds.Left <= o.X && txt.Bounds.Right >= o.X + o.Width));
                                        if (title == null)
                                        {
                                            break;
                                        }
                                        if (title.Type == SearchTitleEnum.BH)
                                        {
                                            searchRows.Add(new SearchRow()
                                            {
                                                X = txt.Bounds.X,
                                                Y = txt.Bounds.Y,
                                                Id = txt.SearchText
                                            });
                                            search.SearchContents.Add(new SearchContent()
                                            {
                                                Id = txt.SearchText
                                            });
                                        }
                                        var bhTitle = searchTitles.FirstOrDefault(o => o.Type == SearchTitleEnum.BH);
                                        var searchRow = new SearchRow();
                                        var searchIndex = 0;
                                        while (searchIndex <= 2)
                                        {
                                            searchIndex++;
                                            searchRow = searchRows.FirstOrDefault(r => r.Y - (8 * searchIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * searchIndex));
                                            if (searchRow != null)
                                            {
                                                break;
                                            }
                                        }
                                        if (searchRow == null || string.IsNullOrEmpty(searchRow.Id))
                                        {
                                            break;
                                        }
                                        var searchContent = search.SearchContents.FirstOrDefault(o => o.Id == searchRow.Id);
                                        switch (title.Type)
                                        {
                                            case SearchTitleEnum.BH:
                                                searchContent.Id = txt.SearchText;
                                                break;
                                            case SearchTitleEnum.TIME:
                                                searchContent.Time += txt.SearchText;
                                                break;
                                            case SearchTitleEnum.CZY:
                                                searchContent.CZY += txt.SearchText;
                                                break;
                                            case SearchTitleEnum.YY:
                                                searchContent.YY += txt.SearchText;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                }
                            }
                            else if (isSeachONE)
                            {
                                var search = result.SeachList.Seachs.First(o => o.SearchType == SearchManType.ONE);
                                switch (trimTxt)
                                {
                                    case "编号":
                                        searchTitles = new List<SearchTitle>();
                                        searchTitles.Add(new SearchTitle()
                                        {
                                            Type = SearchTitleEnum.BH,
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Width = txt.Bounds.Width
                                        });
                                        break;
                                    case "查询日期":
                                        searchTitles.Add(new SearchTitle()
                                        {
                                            Type = SearchTitleEnum.TIME,
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Width = txt.Bounds.Width
                                        });
                                        break;
                                    case "查询操作员":
                                        searchTitles.Add(new SearchTitle()
                                        {
                                            Type = SearchTitleEnum.CZY,
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Width = txt.Bounds.Width
                                        });
                                        break;
                                    case "查询原因":
                                        searchTitles.Add(new SearchTitle()
                                        {
                                            Type = SearchTitleEnum.YY,
                                            X = txt.Bounds.X,
                                            Y = txt.Bounds.Y,
                                            Width = txt.Bounds.Width
                                        });
                                        break;
                                    default:
                                        var title = searchTitles.FirstOrDefault(o => (o.X <= txt.Bounds.X && txt.Bounds.X <= o.X + o.Width) || (txt.Bounds.Left <= o.X && txt.Bounds.Right >= o.X + o.Width));
                                        if (title == null)
                                        {
                                            break;
                                        }
                                        if (title.Type == SearchTitleEnum.BH)
                                        {
                                            searchRows.Add(new SearchRow()
                                            {
                                                X = txt.Bounds.X,
                                                Y = txt.Bounds.Y,
                                                Id = txt.SearchText
                                            });
                                            search.SearchContents.Add(new SearchContent()
                                            {
                                                Id = txt.SearchText
                                            });
                                        }
                                        var bhTitle = searchTitles.FirstOrDefault(o => o.Type == SearchTitleEnum.BH);
                                        var searchRow = new SearchRow();
                                        var searchIndex = 0;
                                        while (searchIndex <= 2)
                                        {
                                            searchIndex++;
                                            searchRow = searchRows.FirstOrDefault(r => r.Y - (8 * searchIndex) <= txt.Bounds.Y && txt.Bounds.Y <= r.Y + (8 * searchIndex));
                                            if (searchRow != null)
                                            {
                                                break;
                                            }
                                        }
                                        if (searchRow == null || string.IsNullOrEmpty(searchRow.Id))
                                        {
                                            break;
                                        }
                                        var searchContent = search.SearchContents.First(o => o.Id == searchRow.Id);
                                        switch (title.Type)
                                        {
                                            case SearchTitleEnum.BH:
                                                searchContent.Id = txt.SearchText;
                                                break;
                                            case SearchTitleEnum.TIME:
                                                searchContent.Time += txt.SearchText;
                                                break;
                                            case SearchTitleEnum.CZY:
                                                searchContent.CZY += txt.SearchText;
                                                break;
                                            case SearchTitleEnum.YY:
                                                searchContent.YY += txt.SearchText;
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                    #endregion
                    response.data = result;
                    response.code = 1;
                    response.msg = "读取成功";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.code = -1;
                response.msg = $"读取文件错误{ex.Message}";
                return response;
            }
        }
        /// <summary>
        /// 简版征信多上传
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        [HttpPost("JBReadByFiles")]
        public List<PDFResponse> JBReadByFiles(IFormFileCollection files)
        {
            var result = new List<PDFResponse>();
            foreach (var file in files)
            {
                result.Add(JBReadByFile(file));
            }
            return result;
        }
        /// <summary>
        /// 通过byte[]获取pdf内容
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("ReadByBytes")]
        public string ReadByBytes(IFormFile file)
        {
            if (file == null)
            {
                return "请上传文件";
            }
            var dir = "pdf";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var filePath = Path.Combine(dir, file.FileName);
            using (FileStream fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
                byte[] pReadByte = new byte[0];
                BinaryReader r = new BinaryReader(fs);
                r.BaseStream.Seek(0, SeekOrigin.Begin);    //将文件指针设置到文件开
                pReadByte = r.ReadBytes((int)r.BaseStream.Length);

                PdfDocument pdf = new PdfDocument();
                pdf.LoadFromBytes(pReadByte);
                var sb = new StringBuilder();
                foreach (PdfPageBase page in pdf.Pages)
                {
                    string text = page.ExtractText(new RectangleF(0, 0, 1000, 1000));
                    var spStr = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    //清除第一和最后一个：无效
                    var spStrs = spStr.Skip(1).Take(spStr.Count() - 2).ToList();
                    foreach (var sp in spStrs)
                    {
                        sb.Append(sp);
                    }
                }
                fs.Flush();
                return sb.ToString();
            }
        }
        [HttpGet(@"ReadImageByPath/{filePath=G:\1.pdf}/{imageNumber=0}")]
        public FileResult ReadImageByPath(string filePath = @"G:\1.pdf", int pageIndex = 0, int imageNumber = 0)
        {
            filePath = filePath.Replace("%2F", "\\");
            //创建PdfDocument实例
            PdfDocument pdf = new PdfDocument();
            //加载PDF文档
            pdf.LoadFromFile(filePath);
            var sb = new StringBuilder();
            if (pdf.Pages.Count < pageIndex + 1)
            {//页面不存在
                return null;
            }
            Image[] ss = pdf.Pages[pageIndex].ExtractImages();
            if (ss != null && imageNumber < ss.Count())
            {
                var imageFirst = ss[imageNumber];
                Bitmap bmp = new Bitmap(ss[imageNumber]);
                //bmp.Save("G:\\name.png");
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                bmp.Dispose();

                FileResult fileResult = new FileContentResult(ms.ToArray(), "image/png");
                return fileResult;

                //return File(ms, "image/png");
            }
            return null;
        }
        /// <summary>
        /// 获取pdf内的图片
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pageIndex"></param>
        /// <param name="imageNumber"></param>
        /// <returns></returns>
        [HttpPost("ReadImageByFile")]
        public ActionResult ReadImageByFile(IFormFile file, [FromForm]int pageIndex = 0, [FromForm]int imageNumber = 0)
        {
            if (file == null)
            {
                return Ok(new { code = 200, msg = "未上传图片" });
            }
            var dir = "pdf";
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, file.FileName);
            using (FileStream fs = System.IO.File.Create(filePath))
            {
                file.CopyTo(fs);
                fs.Flush();
                PdfDocument pdf = new PdfDocument();
                pdf.LoadFromStream(fs);
                if (pdf.Pages.Count < pageIndex + 1)
                {//页面不存在
                    return Ok(new { code = 200, msg = "页面不存在" });
                }
                Image[] ss = pdf.Pages[pageIndex].ExtractImages();
                if (ss != null && imageNumber < ss.Count())
                {
                    var imageFirst = ss[imageNumber];
                    Bitmap bmp = new Bitmap(ss[imageNumber]);
                    var dirs = "images";
                    if (Directory.Exists(dirs))
                    {
                        Directory.Delete(dirs, true);
                    }
                    Directory.CreateDirectory(dirs);
                    var imageName = "1.png";
                    bmp.Save(dirs + "/" + imageName);
                    bmp.Dispose();
                    return Ok(new { code = 200, msg = "成功", Url = this.Request.Scheme + "://" + this.Request.Host + "/" + dirs + "/" + imageName });
                }
                return Ok(new { code = 200, msg = "找不到图片" });
            }
        }

    }
}
