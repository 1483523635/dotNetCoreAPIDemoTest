
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TestAPI.Models;

namespace TestAPI.Controllers
{
    public class UsersController : Controller
    {
        #region 成员变量

        private static string baseURL = "http://localhost:56853/api/users";
        private static IList<Users> _context;
        static HttpClient client = new HttpClient();
        #endregion

        #region 构造函数
        public UsersController()
        {
            _context = new List<Users>();
            _context = GetAllUserList();

        }

        #endregion

        #region 类内方法

       
        ///  https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/calling-a-web-api-from-a-net-client
      


        /// <summary>
        /// 通过api获取所有的用户的信息
        /// </summary>
        /// <returns>JSON String</returns>
        private string GetALLUserInfoFromAPI()
        {
            var con = client.GetStringAsync(baseURL);
            return con.Result;
        }

        private string UsersConvertToJson(Users u)
        {
            return JsonConvert.SerializeObject(u);
        }
        /// <summary>
        /// 添加数据到数据库中
        /// </summary>
        /// <param name="u"></param>
        /// <returns>添加的一条数据json串</returns>
        private async Task<string> CreateAsync(Users u)
        {
            string url = baseURL;
            string requestMethod = "post";
            return await HandleData(u, url, requestMethod);         
        }

        private async Task<string> UpdateAsync(Users u)
        {
            string url = baseURL + @"/" + u.ID;
            string requestMethod = "put";
            return await HandleData(u, url, requestMethod);
        }

        private async Task<string> HandleData(Users Data, string Url, string RequestMethod)
        {
            string UsersJson = UsersConvertToJson(Data);
            var request = WebRequest.CreateHttp(Url);
            request.Accept = "application/json";
            //下边这行不设置会出现无法识别mediaType 415 这个错误
            request.ContentType = "application/json";
            request.Method = RequestMethod;
            //向request提交数据
            using (StreamWriter writer = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                writer.Write(UsersJson);
            }
            //获取响应
            var reponse = await request.GetResponseAsync();
            //返回响应数据
            using (StreamReader reader = new StreamReader(reponse.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
        }
        private async Task<string> DeleteAsync(Users u)
        {
            string url = baseURL + @"/" + u.ID;
            string requestMethod = "delete";
            return await HandleData(u, url, requestMethod);
        }
        private bool UsersExist(int iD)
        {
            return _context.Any(u => u.ID == iD);
        }

        /// <summary>
        /// 获取所有用户的List
        /// </summary>
        private IList<Users> GetAllUserList()
        {
            IList<Users> userslist = new List<Users>();
            var JsonString = GetALLUserInfoFromAPI();
            JArray UsersArray = JArray.Parse(JsonString);
            for (int i = 0; i < UsersArray.Count; i++)
            {
                userslist.Add(StringConvertToUser(UsersArray[i].ToString()));
            }
            return userslist;
        }
        /// <summary>
        /// 将Json对象的字符串转化为users对象
        /// </summary>
        /// <param name="JsonString">json对象的字符串</param>
        /// <returns>Users对象</returns>
        private Users StringConvertToUser(string JsonString)
        {
            return JsonConvert.DeserializeObject<Users>(JsonString);
        }
        #endregion

        #region Index
        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(_context);
        }



        #endregion

        #region Details

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var users = _context.FirstOrDefault(u => u.ID == id);
            if (users == null)
            {
                return NotFound();
            }
            return View(users);
        }

        #endregion

        #region Create

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,name,pwd")] Users users)
        {
            if (ModelState.IsValid)
            {
               
                await CreateAsync(users);
                return RedirectToAction("Index");
            }
            return View();
        }

        #endregion

        #region Edit

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var users = _context.FirstOrDefault(u => u.ID == id);
            if (users == null)
            {
                return NotFound();
            }
            return View(users);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,name,pwd")] Users users)
        {

            if (ModelState.IsValid)
            {
                if (id != users.ID)
                {
                    return BadRequest();
                }
                try
                {
                    await UpdateAsync(users);
                    return RedirectToAction("Index");
                }
                catch
                {
                    if (UsersExist(users.ID))
                    {
                        return NotFound();
                    }
                    throw;
                }

            }
            return View();
        }


        #endregion

        #region Delete


        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var users = _context.FirstOrDefault(u => u.ID == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var users = _context.SingleOrDefault(u => u.ID == id);
            await DeleteAsync(users);
            return RedirectToAction("Index");
        }
        #endregion

    }
}
