using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using X.PagedList;
using Microsoft.AspNetCore.Hosting;
using prjIHealth.Models;
using Microsoft.AspNetCore.Http;
using prjiHealth.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace prjIHealth.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    public class NewsManageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        private IWebHostEnvironment _enviroment;
        public NewsManageController(IWebHostEnvironment n)
        {
            _enviroment = n;
        }

        public IActionResult PageList()
        {
            //歐付寶刷卡金流測試內容
            //https://developers.opay.tw/Prepare/Intro 測試環境設定介紹
            //http://developers.opay.tw/AioCreditCard/CreateOrder 信用卡線上即時模擬說明

            string randomNumber = Guid.NewGuid().ToString();//產生編號亂數
            string merchantTradeNo = randomNumber.Substring(0,24).Replace("-","");//取訂單編號格式二十碼和去掉-符號
            ViewBag.MerchantTradeNo = merchantTradeNo;//輸出至前端
            string merchantTradeDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");//現在時間格式
            ViewBag.TradeDate = merchantTradeDate;//輸出至前端
            string itemName = "測試";//商品名稱代入此處
            ViewBag.ItemName = itemName;//輸出至前端
            int totalAmount = 5;//付款總金額
            ViewBag.TotalAmout = totalAmount;//輸出至前端

            string checkMacValue1 = $"HashKey=5294y06JbISpM5x9&ChoosePayment=Credit&ClientBackURL=" +
                $"https://developers.opay.tw/AioMock/MerchantClientBackUrl&CreditInstallment=&EncryptType=1&" +
                $"InstallmentAmount=&ItemName={itemName}&MerchantID=2000132&MerchantTradeDate={merchantTradeDate}&" +
                $"MerchantTradeNo={merchantTradeNo}&PaymentType=aio&Redeem=&ReturnURL=https://developers.opay.tw/" +
                $"AioMock/MerchantReturnUrl&StoreID=&TotalAmount={totalAmount}&TradeDesc=建立信用卡測試訂單&" +
                $"HashIV=v77hoKGq4kWxNNIS";
            string checkMacValue2 = System.Web.HttpUtility.UrlEncode(checkMacValue1, Encoding.UTF8).ToLower();//轉為UTF-8的編碼UrlEncode完轉小寫
            //string checkMacValue3 = checkMacValue2.ToLower();//轉為小寫

            using var hashCode = SHA256.Create(); //建立SHA256個體
            var hashingCode = hashCode.ComputeHash(Encoding.UTF8.GetBytes(checkMacValue2));//計算指定雜湊值

            string checkMacValue3 = Convert.ToHexString(hashingCode).ToUpper();//轉換為使用大寫十六進位字元編碼的相等字串表示法，轉為大寫
            //string checkMacValue5 = checkMacValue4.ToUpper(); //轉為大寫最後一步
            ViewBag.CheckMacValue = checkMacValue3;//輸出至前端

            return View();
        }


            //IEnumerable<TNews> datas = null;
            //if (string.IsNullOrEmpty(vModel.txtKeyword))
            //{
            //    datas = from t in db.TNews
            //            select t;
            //    //data = db.TNews.OrderBy(t => t.FNewsId).ToList();
            //    //datas = db.TNews.ToPagedList(currentPage, pageListSize);
            //}
            //else
            //{
            //    datas = db.TNews.Where(t => t.FTitle.Contains(vModel.txtKeyword));
            //}
            //return View(datas);
            //}
            //int pageBlogSize = 6;
        public IActionResult List(int page = 1)
        {
            //int currentPage = vModel.page < 1 ? 1 : vModel.page;
            IHealthContext db = new IHealthContext();
            int currentPage = page < 1 ? 1 : page;
            // Trace.WriteLine(db.TNews);
            var news = db.TNews.OrderBy(n => n.FNewsId).ToList();
            if (news != null)
            {
                var result = news.ToPagedList(currentPage, 4);
                return View(result);
            }
            return View();
        }

        public IActionResult Details(int? id)
        {
            IHealthContext db = new IHealthContext();
            TNews news = db.TNews.FirstOrDefault(t => t.FNewsId == id);
            if (news == null)
                return RedirectToAction("List");
            return View(news);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(CNewsViewModel vModel)
        {
            IHealthContext db = new IHealthContext();
            TNews news = new TNews();
            news.FTitle = vModel.FTitle;
            news.FContent = vModel.FContent;
            news.FNewsDate = vModel.FNewsDate;
            news.FNewsCategoryId = vModel.FNewsCategoryId;
            news.FVideoUrl = vModel.FVideoUrl;
            news.FViews = vModel.FViews;
            news.FMemberId = vModel.FMemberId;

            if (vModel.photo != null)
            {
                string nName = Guid.NewGuid().ToString() + ".jpg";
                vModel.photo.CopyTo(new FileStream(
                    _enviroment.WebRootPath + "/img/blog/" + nName, FileMode.Create));
                news.FThumbnailPath = nName;
            }
            db.Add(news);
            db.SaveChanges();

            return RedirectToAction("List");
        }

        public IActionResult Edit(int? id)
        {
            IHealthContext db = new IHealthContext();
            TNews news = db.TNews.FirstOrDefault(t => t.FNewsId == id);
            if (news == null)
                return RedirectToAction("List");
            return View(news);
        }
        [HttpPost]
        public IActionResult Edit(CNewsViewModel n)
        {
            IHealthContext db = new IHealthContext();
            // foreach (var dbNew in db.TNews)
            // {
            //     Console.WriteLine(dbNew.FNewsId);
            //     Console.WriteLine(n.FNewsId);
            // }
            TNews news = db.TNews.FirstOrDefault(t => t.FNewsId == n.FNewsId);
            if (news != null)
            {
                if (n.photo != null)
                {
                    string nName = Guid.NewGuid().ToString() + ".jpg";
                    n.photo.CopyTo(new FileStream(
                        _enviroment.WebRootPath + "/img/blog/" + nName, FileMode.Create));
                    news.FThumbnailPath = nName;
                }
                news.FTitle = n.FTitle;
                news.FNewsDate = n.FNewsDate;
                news.FContent = n.FContent;
                news.FNewsCategoryId = n.FNewsCategoryId;
                news.FVideoUrl = n.FVideoUrl;
            }
            db.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            IHealthContext db = new IHealthContext();
            var news = db.TNews.FirstOrDefault(t => t.FNewsId == id);
            if (news != null)
            {
                db.TNews.Remove(news);
                db.SaveChanges();
            }
            return RedirectToAction("List");
        }

        //[HttpPost("FileUpload")]
        //public async Task<IActionResult> Index(List<IFormFile> files)
        //{
        //    long size = files.Sum(f => f.Length);

        //    var filePaths = new List<string>();
        //    foreach (var formFile in files)
        //    {
        //        if (formFile.Length > 0)
        //        {
        //            // full path to file in temp location
        //            var filePath = Path.GetTempFileName();
        //            filePaths.Add(filePath);

        //            using (var stream = new FileStream(filePath, FileMode.Create))
        //            {
        //                await formFile.CopyToAsync(stream);
        //            }
        //        }
        //    }
        //    // process uploaded files
        //    // Don't rely on or trust the FileName property without validation.
        //    return Ok(new { count = files.Count, size, filePaths });
        //}
    }
}
