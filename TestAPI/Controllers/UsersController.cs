
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
        #region ��Ա����
        //���baseURL���ҵ�webApi�ĵ�ַ
        private static string baseURL = "http://localhost:56853/api/users";
        //���ڴ�����е��û�
        private static IList<Users> _context;

        #endregion

        #region ���캯��
        public UsersController()
        {
            //ʵ��������
            _context = new List<Users>();
            //��ȡ���е��û�����Ϣ
            _context = GetAllUserList();

        }

        #endregion

        #region ���ڷ���

        /// <summary>
        /// ͨ��api��ȡ���е��û�����Ϣ
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
        /// �������ݵ�Զ��API�����ݿ���
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
        /// ��������
        /// </summary>
        /// <param name="Data">Userʵ����</param>
        /// <param name="Url">Զ��API��URL</param>
        /// <param name="RequestMethod">����ķ���</param>
        /// <returns></returns>
        private async Task<string> HandleDataAsync(Users Data, string Url, string RequestMethod)
        {
            string UsersJson = UsersConvertToJson(Data);
            var request = WebRequest.CreateHttp(Url);
            request.Accept = "application/json";
            //�±����в����û�����޷�ʶ��mediaType 415 �������
            request.ContentType = "application/json";
            request.Method = RequestMethod;
            //��request�ύ����
            using (StreamWriter writer = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                writer.Write(UsersJson);
            }
            //��ȡ��Ӧ
            var reponse = await request.GetResponseAsync();
            //������Ӧ����
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
        /// ������HttpClient����Web Apiʵ��CRUD�����ݿ��� 
        /// </summary>
        /// <param name="u">����</param>
        /// <param name="url">�����url</param>
        /// <param name="RequestType">���������</param>
        /// <returns></returns>
        private async Task<string> HandleDataFactoryAsync(Users u, string url, string RequestType)
        {
            //����һ��HttpClient��
            HttpClient client = new HttpClient();
            //��Usersת��ΪJSon�ַ���
            string jsonString = UsersConvertToJson(u);
            //����һ��String���͵�Context���ύ����ʱ��Ҫ�õ��������
            var context = new StringContent(jsonString, Encoding.UTF8, "application/json");
            //���÷�����Ҫ��ȡ�ķ�������
            string method = RequestType + "Async";

            //�����ｫ����д����һ��
            //1.ͨ����ȡ���еķ���
            //2.ͨ��linq����ѯ����Ҫ�ķ���
            //��ȻҲ����ֱ��GetMethod(MethodName)
            //�Ҹо������Ļ����ܻ��ס������Ҳ��������쳣
            //����Linq��ѯ������
            MethodInfo info = client.GetType().GetMethods().FirstOrDefault(m => m.Name == method);
            if (info != null)
            {
                HttpResponseMessage result;

                //����put��post��Ҫ�Ĳ������ͺ���Ŀһ�����Է���һ�鴦���� 
                //get��deleteҲһ��
                if (method == "PutAsync" || method == "PostAsync")
                {
                    //��������������Ҵ�������Ҫ�Ĳ���
                     result= await (Task<HttpResponseMessage>)info.Invoke(client, new Object[] { url, context });   
                }
                else
                {
                    result = await (Task<HttpResponseMessage>)info.Invoke(client, new Object[] { url });
                }
                //����Ӧ���ݷ���
                return await result.Content.ReadAsStringAsync();

            }


            return "";

        }
        /// <summary>
        /// ��ȡ�����û���List
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
        /// ��Json������ַ���ת��Ϊusers����
        /// </summary>
        /// <param name="JsonString">json������ַ���</param>
        /// <returns>Users����</returns>
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
