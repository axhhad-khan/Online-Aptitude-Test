using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.IO;
using System.Collections;
using System.Net;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        QuizAppEntities db = new QuizAppEntities();

        

        [HttpGet]
        public ActionResult sregister()
        {
            return View();
        }
        [HttpPost]
        public ActionResult sregister(student svm, HttpPostedFileBase imgfile)
        {
            student s = new student();

            try
            {
                s.std_name = svm.std_name;
                s.std_password = svm.std_password;
                s.std_image = uploadimage(imgfile);
                db.students.Add(s);
                db.SaveChanges();
                return RedirectToAction("students");
            }
            catch (Exception)
            {

                ViewBag.msg = "Data could not be inserted...";
            }


            return View();
        }

        public string uploadimage(HttpPostedFileBase file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {
                        path = Path.Combine(Server.MapPath("/Content/img/"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);
                        //    ViewBag.Message = "File uploaded successfully"
                    }
                    catch (Exception ex)
                    {
                        path = "-1";
                    }
                }
                else
                {
                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");
                }
            }
            else
            {
                Response.Write("<script>alert('Please select a file'); </script>");
                path = "-1";
            }
            return path;
        }

        public ActionResult LogOut()
        {
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Index");
        }
        public ActionResult Lo()
        {
            Session.Abandon();
            Session.RemoveAll();
            return RedirectToAction("Index");
        }

 
        public ActionResult mlogin()
        {
            return View();

        }

        [HttpPost]
        public ActionResult mlogin(tbl_admin a)
        {
            tbl_admin ad = db.tbl_admin.Where(x => x.ad_name == a.ad_name && x.ad_pass == a.ad_pass).SingleOrDefault();
            //tbl_admin ad = db.tbl_admin.Where(x => x.ad_name == a.ad_name && x.ad_pass == a.ad_pass).SingleOrDefault();
            if (ad!=null)
            {
                Session["ad_id"] = ad.ad_id; 
                return RedirectToAction("Dashboard");
            }
           else
            {
                ViewBag.msg = "Invalid Username or Password";
            }
            return View();
        }

        [HttpGet]
        public ActionResult slogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult slogin(student ss)
        {
            //student sv = db.students.Where(y => y.std_name == ss.std_name && y.std_password == ss.std_password).SingleOrDefault();
            //if (sv != null)
            //{
            //    Session["std_id"] = sv.std_id;
            //    return RedirectToAction("Examdashboard");
            //}
            //else
            //{
            //    ViewBag.msg = "Invalid Username or Password";
            //}
            //return View();

            student sv = db.students.Where(x => x.std_name == ss.std_name && x.std_password == ss.std_password).SingleOrDefault();
            if (sv == null)
            {
                ViewBag.msg = "Invalid Username or Password";
            }
            else
            {
                Session["std_id"] = ss.std_id;
                return RedirectToAction("Examdashboard");
            }
            return View();
        }
        public ActionResult Examdashboard()
        {
            //if (Session["std_id"] != null)
            //{
            //    return RedirectToAction("slogin");
            //}
            return View();
        }

        [HttpPost]
        public ActionResult Examdashboard(string room)
        {
            List<tbl_category> list = db.tbl_category.ToList();
            foreach (var item in list)
            {
                if (item.cat_encrytped_string == room)
                {
                    List<tbl_questions> li = db.tbl_questions.Where(x => x.q_fk_cat_id == item.cat_id).ToList();
                    Queue<tbl_questions> queue = new Queue<tbl_questions>();
                    foreach (tbl_questions a in li)
                    {
                        queue.Enqueue(a);
                    }

                    TempData["questions"] = queue;
                    TempData["score"] = 0;

                    TempData.Keep();
                    return RedirectToAction("StartQuiz");
                }
                else
                {
                   ViewBag.error = "No Room Found...";
                }
            }
            return View();
        }

        public ActionResult StartQuiz()
        {
            if (Session["std_id"] == null)
            {
                return RedirectToAction("slogin");
            }
            tbl_questions q = null;
            if (TempData["questions"] != null)
            {
                Queue<tbl_questions> qlist = (Queue<tbl_questions>)TempData["questions"];
                if (qlist.Count>0)
                {
                    q = qlist.Peek(); 
                    qlist.Dequeue();

                    TempData["questions"] = qlist;
                    TempData.Keep();
                }
                else
                {
                    return RedirectToAction("EndExam");

                }
            }
            else
            {
                return RedirectToAction("Examdashboard");
            }

            return View(q);
        }


        [HttpPost]
        public ActionResult StartQuiz(tbl_questions q)
        {
            string correctans = null;
            if (q.QA != null)
            {

                correctans = "A";

            }
            else if (q.QB != null)
            {
                correctans = "B";


            }
            else if (q.QC != null)
            {
                correctans = "C";



            }
            else if (q.QD != null)
            {
                correctans = "D";


            }

            if (correctans.Equals(q.QcorrectAns))
            {
                TempData["score"] = Convert.ToInt32(TempData["score"]) + 1;

            }
            TempData.Keep();

            return RedirectToAction("StartQuiz");
        }

        public ActionResult ViewAllQuestions(int ?id)
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("mlogin");
            }
            if (id == null)
            {
                return RedirectToAction("Dashboard");
            }
            return View(db.tbl_questions.Where(x => x.q_fk_cat_id == id).ToList());
        }

        public ActionResult EndExam(tbl_questions q)
        {
            return View();
        }


        public ActionResult Dashboard()
        {
            //if (Session["ad_id"] != null)
            //{
            //    return RedirectToAction("Dashboard");
            //}
            return View();
        }

        

        [HttpGet]
        public ActionResult Add_Category()
        {
            //Session["ad_id"] = 1;
            int admin_id = Convert.ToInt32(Session["ad_id"].ToString());
            List<tbl_category> cat_li = db.tbl_category.Where(x=>x.cat_fk_ad_id == admin_id).OrderByDescending(x => x.cat_id).ToList();
            ViewData["list"] = cat_li;
            return View();
        }
        [HttpPost]
        public ActionResult Add_Category(tbl_category cat)
        {
            List<tbl_category> cat_li = db.tbl_category.OrderByDescending(x => x.cat_id).ToList();
            ViewData["list"] = cat_li;
            tbl_category c = new tbl_category();

            Random r = new Random();

            c.cat_name = cat.cat_name;
            c.cat_fk_ad_id = Convert.ToInt32(Session["ad_id"].ToString());
            c.cat_encrytped_string = crypt.Encrypt(cat.cat_name.Trim() + r.Next().ToString(),true);

            db.tbl_category.Add(c);
            db.SaveChanges();
            return RedirectToAction("Add_Category");
        }

        [HttpGet]
        public ActionResult Add_Ques()
        {

            int category_id = Convert.ToInt32(Session["ad_id"]);
            List<tbl_category> li = db.tbl_category.Where(x => x.cat_id == category_id).ToList();
            ViewBag.List = new SelectList(li, "cat_id", "cat_name");
            return View();
        }

        [HttpPost]
        public ActionResult Add_Ques(tbl_questions q)
        {

            int category_id = Convert.ToInt32(Session["ad_id"]);
            List<tbl_category> li = db.tbl_category.Where(x => x.cat_id == category_id).ToList();
            ViewBag.List = new SelectList(li, "cat_id", "cat_name");

            tbl_questions qa = new tbl_questions();
            qa.q_text = q.q_text;
            qa.QA = q.QA;
            qa.QB = q.QB;
            qa.QC = q.QC;
            qa.QD = q.QD;
            qa.QcorrectAns = q.QcorrectAns; 
            qa.q_fk_cat_id = q.q_fk_cat_id;
            db.tbl_questions.Add(qa);
            db.SaveChanges();

            TempData["ms"] = "Question Successfully Added";
            TempData.Keep();
            return RedirectToAction("Add_Ques");
        }

        public ActionResult Index()
        {
            if (Session["ad_id"] != null)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }
        public ActionResult Delete(FormCollection fcNotUsed, int id = 0)
        {
            tbl_questions ques = db.tbl_questions.Find(id);
            if (ques == null)
            {
                return HttpNotFound();
            }
            
            db.tbl_questions.Remove(ques);
            db.SaveChanges();
            return RedirectToAction("Add_Category");
        } 
        public ActionResult Del(FormCollection fcNotUsed, int id = 0)
        {
            tbl_category cat = db.tbl_category.Find(id);
            if (cat == null)
            {
                return HttpNotFound();
            }
            db.tbl_category.Remove(cat);
            db.SaveChanges();
            return RedirectToAction("Add_Category");
        }
        
        public ActionResult Dell(FormCollection fcNotUsed, int id = 0)
        {
            student std = db.students.Find(id);
            if (std == null)
            {
                return HttpNotFound();
            }
            db.students.Remove(std);
            db.SaveChanges();
            return RedirectToAction("students");
        }



        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult students()
        {
            return View(db.students.ToList());
        }


    }
}