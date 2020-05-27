using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Computer_Shop.Models;
using Microsoft.Ajax.Utilities;

namespace Computer_Shop.Controllers
{
    public class HomeController : Controller
    {
        Computer_Management_SystemEntities db = new Computer_Management_SystemEntities();

        /*===========================================================================*/
        /*======================= MAIN FUNCTION =======================================*/
        /*===========================================================================*/
        public ActionResult Index()
        {
            return View();
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

        /*===========================================================================*/
        /*======================= SHOP FUNCTION =======================================*/
        /*===========================================================================*/

        public ActionResult Shop()
        {
            List<Product> list = db.Products.ToList();
            return View(list);
        }

        public ActionResult Cart()
        {
            return View();
        }

        public ActionResult Checkout()
        {
            return View();
        }

        public ActionResult My_account()
        {
            return View();
        }

        public ActionResult Wishlist()
        {
            return View();
        }

        /*===========================================================================*/
        /*======================= ACCOUNT FUNCTION =======================================*/
        /*===========================================================================*/

        /*== LOGIN ==*/
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Account account,Employee employee)
        {
            try
            {
                var user = db.Accounts.SingleOrDefault(u => u.UserName.Equals(account.UserName));
                if (user != null)
                {
                    if (user.Password.Equals(account.Password))
                    {
                        Session["uname"] = user.UserName;
                        Session["uid"] = user.UserID;
                        if (user.Role == 1)
                        {
                            Session["role"] = "admin";
                        }
                        else if (user.Role == 2)
                        {
                            Session["role"] = "Storage Manager";
                        }
                        else if (user.Role == 3)
                        {
                            Session["role"] = "Salesman";
                        }
                        else if (user.Role == 4)
                        {
                            Session["role"] = "Accounting";
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.Message = "Incorrect Password or username !";
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Home");
                }
            }
            catch (Exception e)
            {
                ViewBag.Msg = e.Message;
            }
            return View();
        }

        /*== CREATE ACCOUNT ==*/
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Register(Account account)
        {
            if (ModelState.IsValid)
            {
                var check = db.Accounts.FirstOrDefault(u => u.UserName.Equals(account.UserName));
                if (check == null)
                {
                    account.Role = account.Role;
                    account.Password = account.Password;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Accounts.Add(account);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.error = "Username already exists";
                    return View();
                }


            }
            return View();
        }

        public ActionResult AddCustomer()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Customers.Add(customer);
                    db.SaveChanges();
                    return RedirectToAction("Customer","Home");
            }
            return View();
        }

        /*== LOGOUT ==*/
        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();  /*Xóa hết session đang thực thi*/
            return RedirectToAction("Index");
        }

        /*===========================================================================*/
        /*======================= PRODUCT FUNCTION =======================================*/
        /*===========================================================================*/
        
        public ActionResult AddProduct()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Index","Home");
            }
            else if(Session["uname"] != null && Session["role"] != "admin" && Session["role"] != "Storage Manager")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                Session["pid"] = product.Product_ID;
                string filename1 = Path.GetFileNameWithoutExtension(product.ImageFile1.FileName);
                string filename2 = Path.GetFileNameWithoutExtension(product.ImageFile2.FileName);
                string filename3 = Path.GetFileNameWithoutExtension(product.ImageFile3.FileName);
                string extension1 = Path.GetExtension(product.ImageFile1.FileName);
                string extension2 = Path.GetExtension(product.ImageFile2.FileName);
                string extension3 = Path.GetExtension(product.ImageFile3.FileName);
                filename1 = filename1 + DateTime.Now.ToString("yymmssff") + extension1;
                filename2 = filename2 + DateTime.Now.ToString("yymmssff") + extension2;
                filename3 = filename3 + DateTime.Now.ToString("yymmssff") + extension3;
                product.Image1 = "/Content/Shop/images/" + filename1;
                product.Image2 = "/Content/Shop/images/" + filename2;
                product.Image3 = "/Content/Shop/images/" + filename3;
                filename1 = Path.Combine(Server.MapPath("/Content/Shop/images/"), filename1);
                filename2 = Path.Combine(Server.MapPath("/Content/Shop/images/"), filename2);
                filename3 = Path.Combine(Server.MapPath("/Content/Shop/images/"), filename3);
                product.ImageFile1.SaveAs(filename1);
                product.ImageFile2.SaveAs(filename2);
                product.ImageFile3.SaveAs(filename3);
                var check = db.Products.FirstOrDefault(u => u.Product_Name.Equals(product.Product_Name));
                if (check == null)
                {
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.Products.Add(product);
                    db.SaveChanges();
                    ModelState.Clear();
                    return RedirectToAction("Shop");
                }
            }
            return View();     
        }

        [HttpGet]  /*== Viewing EACH Product ==*/
        public ActionResult Product(int id)
        {
            var product = db.Products.SingleOrDefault(u => u.Product_ID.Equals(id));
            return View(product);
        }

        public ActionResult DeleteProduct(int id)
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else if (Session["uname"] != null && Session["role"] != "admin")
            {
                return RedirectToAction("Index", "Home");
            }
            var product = db.Products.SingleOrDefault(u => u.Product_ID.Equals(id));
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Shop", "Home");
        }

        public ActionResult EditProduct(int id)
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else if (Session["uname"] != null && Session["role"] != "admin")
            {
                return RedirectToAction("Index", "Home");
            }
            var product = db.Products.SingleOrDefault(u => u.Product_ID.Equals(id));
            return View(product);
        }

        [HttpPost,ActionName("EditProduct")]
        public ActionResult EditProduct(Product p)
        {
            var product = db.Products.SingleOrDefault(u => u.Product_ID.Equals(p.Product_ID));
            product.Product_Name = p.Product_Name;
            product.Product_ID = p.Product_ID;
            product.Made_in = p.Made_in;
            product.Quantity = p.Quantity;
            product.Warranty_Period = p.Warranty_Period;
            product.Price = p.Price;
            product.Image1 = p.Image1;
            product.Image2 = p.Image2;
            product.Image3 = p.Image3;
            db.SaveChanges();
            return RedirectToAction("Shop","Home");
        }


        /*===========================================================================*/
        /*======================= USER FUNCTION =======================================*/
        /*===========================================================================*/

            /*======================= Admin FUNCTION =======================================*/
        public ActionResult Admin()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.AccountEmployees.Where(m => m.Role.Equals(1));
            return View(list);
        }

        public ActionResult Employee()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.Accounts.ToList<Account>();
            return View(list);
        }

        public ActionResult Customer()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.Customers.ToList<Customer>();
            return View(list);
        }

        public ActionResult StorageManager()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.AccountEmployees.Where(m=>m.Role.Equals(2));
            return View(list);
        }

        public ActionResult Accounting()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.AccountEmployees.Where(m => m.Role.Equals(4));
            return View(list);
        }

        public ActionResult Salesman()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.AccountEmployees.Where(m => m.Role.Equals(3));
            return View(list);
        }
        public ActionResult User()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.Accounts.ToList<Account>();
            return View(list);
        }

        public ActionResult DeleteUser(int UserID)
        {
            var data = db.Accounts.SingleOrDefault(a => a.UserID.Equals(UserID));
            /*  Single() : Returns the only element from a collection, 
            or the only element that satisfies a condition.*/

            db.Accounts.Remove(data);
            /* DeleteOnSubmit() : Dung de xoa cac doi tuong co trong students
             * Khi su dung phai co SubmitChanges() de cap nhat lai du lieu tren SQL Server
             */
            db.SaveChanges();

            return RedirectToAction("User");
            /* Xoa xong load lai danh sach bang hm Redirect, ung voi Index Action*/
        }

        public ActionResult DeleteCustomer(int cID)
        {
            var data = db.Customers.SingleOrDefault(a => a.Customer_ID.Equals(cID));
            /*  Single() : Returns the only element from a collection, 
            or the only element that satisfies a condition.*/

            db.Customers.Remove(data);
            /* DeleteOnSubmit() : Dung de xoa cac doi tuong co trong students
             * Khi su dung phai co SubmitChanges() de cap nhat lai du lieu tren SQL Server
             */
            db.SaveChanges();

            return RedirectToAction("Customer");
            /* Xoa xong load lai danh sach bang hm Redirect, ung voi Index Action*/
        }

        public ActionResult Account(int id)
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var user = db.Accounts.SingleOrDefault(u => u.UserID.Equals(id));
            return View(user);
        }

        public ActionResult ViewAccount(int id)
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var user = db.AccountEmployees.SingleOrDefault(u => u.UserID.Equals(id));
            return View(user);
        }

        [HttpPost, ActionName("ViewAccount")]
        public ActionResult ViewAccount(string UserName, string NewUserName, string Fullname, string NewFullname, string Email, string NewEmail, DateTime DoB, DateTime NewDoB, string Mobile, string NewMob, string City, string NewCiTy, decimal Salary, decimal NewSalary)
        {
            var id = Int32.Parse(Session["uid"].ToString());
            var employe = db.AccountEmployees.SingleOrDefault(u => u.UserID.Equals(id));
            if (employe != null)
            {
                if (NewUserName != UserName)
                {
                    employe.UserName = NewUserName;
                    db.SaveChanges();
                }
                if (NewFullname != Fullname)
                {
                    employe.FullName = NewFullname;
                    db.SaveChanges();
                }
                if (NewEmail != Email)
                {
                    employe.Email = NewEmail;
                    db.SaveChanges();
                }
                if (NewDoB != DoB)
                {
                    employe.DoB = NewDoB;
                    db.SaveChanges();
                }
                if (NewMob != Mobile)
                {
                    employe.Mobile = NewMob;
                    db.SaveChanges();
                }
                if (NewCiTy != City)
                {
                    employe.City = NewCiTy;
                    db.SaveChanges();
                }
                if (NewSalary != Salary)
                {
                    employe.Salary = NewSalary;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ViewEmployee(int Eid)  /*View Employee Information*/
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var employee = db.Employees.SingleOrDefault(u => u.EmployeeID.Equals(Eid));
            Session["Eid"] = employee.EmployeeID;
            return View(employee);
        }

        [HttpPost, ActionName("ViewEmployee")]
        public ActionResult ViewEmployee(string Fullname, string NewFullname, string Email, string NewEmail, DateTime DoB, DateTime NewDoB, string Mobile, string NewMob, string City, string NewCiTy, decimal Salary, decimal NewSalary)
        {
            //================= Get UID to collect user inform ==================================
            var eid = Int32.Parse(Session["Eid"].ToString());
            var employe = db.Employees.SingleOrDefault(u => u.EmployeeID.Equals(eid));
            if (employe != null)
            {
                if (NewFullname != Fullname)
                {
                    employe.FullName = NewFullname;
                    db.SaveChanges();
                }
                if (NewEmail != Email)
                {
                    employe.Email = NewEmail;
                    db.SaveChanges();
                }
                if (NewDoB != DoB)
                {
                    employe.DoB = NewDoB;
                    db.SaveChanges();
                }
                if (NewMob != Mobile)
                {
                    employe.Mobile = NewMob;
                    db.SaveChanges();
                }
                if (NewCiTy != City)
                {
                    employe.City = NewCiTy;
                    db.SaveChanges();
                }
                if (NewSalary != Salary)
                {
                    employe.Salary = NewSalary;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ViewCustomer(int cID)
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var customer = db.Customers.SingleOrDefault(u => u.Customer_ID.Equals(cID));
            Session["cID"] = customer.Customer_ID;
            return View(customer);
        }

        [HttpPost,ActionName("ViewCustomer")]
        public ActionResult ViewCustomer(string Fullname, string NewFullname, string Phonenumber, string NewPhonenumber, string Email, string NewEmail, string Address, string NewAddress)
        {
            var cid = Int32.Parse(Session["cID"].ToString());
            var customer = db.Customers.SingleOrDefault(cus => cus.Customer_ID.Equals(cid));
            if (NewFullname != Fullname)
            {
                customer.Customer_Name = NewFullname;
                db.SaveChanges();
                return RedirectToAction("Customer", "Home");
            }
            if (NewPhonenumber != Phonenumber)
            {
                customer.Phonenumber = NewPhonenumber;
                db.SaveChanges();
                return RedirectToAction("Customer", "Home");
            }
            if (NewEmail != Email)
            {
                customer.Email = NewEmail;
                db.SaveChanges();
                return RedirectToAction("Customer", "Home");
            }
            if (NewAddress != Address)
            {
                customer.Address = NewAddress;
                db.SaveChanges();
                return RedirectToAction("Customer", "Home");
            }

            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Password()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Password(string CurrentPass, string NewPass, string RenewPass)
        {
            var uid = Int32.Parse(Session["uid"].ToString());
            var user = db.Accounts.SingleOrDefault(u => u.UserID.Equals(uid));
            if (user != null)
            {
                if (user.Password.Equals(CurrentPass))
                {
                    if (NewPass.Equals(RenewPass))
                    {
                        user.Password = NewPass;
                        db.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("Password", "Home");
                    }
                    return RedirectToAction("Index", "My_Account");
                }
                return RedirectToAction("Password", "Home");
            }
            return View();
        }

        

        /*===========================================================================*/
        /*======================= ORDER FUNCTION =======================================*/
        /*===========================================================================*/

        public ActionResult Order()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.AccountEmployeeCustomerOrders.ToList();
            return View(list);
        }

        public ActionResult DetailsOrder(int oid)
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var list = db.DetailsOrderProductsOrders.Where(m => m.OrderID.Equals(oid));
            return View(list);
        }

        public ActionResult AddDetailsOrder()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            List<Product> pname = db.Products.ToList();
            List<Product> pprice = db.Products.ToList() ;
            ViewBag.ProductList = new SelectList(pname, "Product_ID", "Product_Name");
            ViewBag.ProductPrice = new SelectList(pprice, "Price", "Price");
            return View();
        }

        [HttpPost]
        public ActionResult AddDetailsOrder(DetailsOrder oid)
        {
            if (ModelState.IsValid)
            {
                db.DetailsOrders.Add(oid);
                db.SaveChanges();
                return RedirectToAction("Order");
            }
            return RedirectToAction("Order", "Home");
        }

        public ActionResult AddOrder()
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            List<Customer> customerID = db.Customers.ToList();
            ViewBag.CustomerList = new SelectList(customerID, "Customer_ID", "Customer_Name",7);
            return View();
        }

        [HttpPost]
        public ActionResult AddOrder(Order oid)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(oid);
                db.SaveChanges();
                return RedirectToAction("Order");
            }
            return RedirectToAction("Order","Home");
        }

        public ActionResult EditOrder(int oid)
        {
            if (Session["uname"] == null)
            {
                return RedirectToAction("Login", "Home");
            }
            var order = db.Orders.SingleOrDefault(u => u.OrderID.Equals(oid));
            return View(order);
        }

        [HttpPost,ActionName("EditOrder")]
        public ActionResult EditOrder(Order o)
        {
            var order = db.Orders.SingleOrDefault(u => u.OrderID.Equals(o.OrderID));
            order.Create_Date = o.Create_Date;
            order.Shipping_Address = o.Shipping_Address;
            order.Status = o.Status;
            order.Payment_Method = o.Payment_Method;
            db.SaveChanges();
            return RedirectToAction("Order", "Home");
        }

        public ActionResult DeleteOrder(int oid)
        {
            var data = db.Orders.SingleOrDefault(a => a.OrderID.Equals(oid));
            db.Orders.Remove(data);
            db.SaveChanges();
            return RedirectToAction("Order");
        }

        public ActionResult DeleteOrderDetails(int doid)
        {
            var datas = db.DetailsOrders.SingleOrDefault(a => a.DetailsOrderID.Equals(doid));
            db.DetailsOrders.Remove(datas);
            db.SaveChanges();
            return RedirectToAction("Order");
        }
    }

}