
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestAPI.Models;

namespace TestAPI.Controllers
{
    public class UsersController : Controller
    {
        #region 成员变量
        //这个baseURL是我的webApi的地址
        private static string baseURL = "http://localhost:56853/api/users";
        //用于存放所有的用户
        private static IList<Users> _context;

        #endregion

        #region 构造函数
        public UsersController()
        {
            //实例化对象
            _context = new List<Users>();
            //获取所有的用户的信息
            _context = GetAllUserList();

        }

        #endregion

        #region 类内方法

        /// <summary>
        /// 通过api获取所有的用户的信息
        /// </summary>
        /// <returns>JSON String</returns>
        private string GetALLUserInfoFromAPI()
        {
            HttpClient client = new HttpClient();
            var con = client.GetStringAsync(baseURL);
            return con.Result;

        }

        private string UsersConvertToJson(Users u)
        {
            return JsonConvert.SerializeObject(u);
        }
        /// <summary>
        /// 传递数据到远程API的数据库中
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        private async Task<string> CreateAsync(Users u)
        {
            string url = baseURL;
              string requestMethod = "Post";
            //return await HandleDataAsync(u, url, requestMethod);
            return await HandleDataFactoryAsync(u, url, requestMethod);
        }

        private async Task<string> UpdateAsync(Users u)
        {
            string url = baseURL + @"/" + u.ID;
            string requestMethod = "Put";
           // return await HandleDataAsync(u, url, requestMethod);
            return await HandleDataFactoryAsync(u, url, requestMethod);
        }
        /// <summary>
        /// 处理数据
        /// </summary>
        /// <param name="Data">User实体类</param>
        /// <param name="Url">远程API的URL</param>
        /// <param name="RequestMethod">请求的方法</param>
        /// <returns></returns>
        private async Task<string> HandleDataAsync(Users Data, string Url, string RequestMethod)
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
            string requestMethod = "Delete";
            //return await HandleDataAsync(u, url, requestMethod);
            return await HandleDataFactoryAsync(u, url, requestMethod);

        }
        private bool UsersExist(int iD)
        {
            return _context.Any(u => u.ID == iD);
        }

        /// <summary>
        /// 这是用HttpClient调用Web Api实现CRUD到数据库中 
        /// </summary>
        /// <param name="u">数据</param>
        /// <param name="url">请求的url</param>
        /// <param name="RequestType">请求的类型</param>
        /// <returns></returns>
        private async Task<string> HandleDataFactoryAsync(Users u, string url, string RequestType)
        {
            //声明一个HttpClient类
            HttpClient client = new HttpClient();
            //将Users转化为JSon字符串
            string jsonString = UsersConvertToJson(u);
            //声明一个String类型的Context在提交数据时需要用到这个参数
            var context = new StringContent(jsonString, Encoding.UTF8, "application/json");
            //设置反射需要获取的方法名称
            string method = RequestType + "Async";

            //我这里将两步写在了一起
            //1.通过获取所有的方法
            //2.通过linq语句查询我需要的方法
            //当然也可以直接GetMethod(MethodName)
            //我感觉那样的话可能会抛“方法找不到”的异常
            //我用Linq查询避免了
            MethodInfo info = client.GetType().GetMethods().FirstOrDefault(m => m.Name == method);
            if (info != null)
            {
                HttpResponseMessage result;

                //由于put和post需要的参数类型和数目一致所以放在一块处理了 
                //get和delete也一样
                if (method == "PutAsync" || method == "PostAsync")
                {
                    //激活这个方法并且传递所需要的参数
                     result= await (Task<HttpResponseMessage>)info.Invoke(client, new Object[] { url, context });   
                }
                else
                {
                    result = await (Task<HttpResponseMessage>)info.Invoke(client, new Object[] { url });
                }
                //将响应内容返回
                return await result.Content.ReadAsStringAsync();

            }


            return "";

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
