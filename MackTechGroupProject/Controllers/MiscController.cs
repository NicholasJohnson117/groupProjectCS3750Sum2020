﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MackTechGroupProject.Controllers
{
    public class MiscController : Controller
    {
        // GET: Misc
        public ActionResult Registration()
        {
            return View();
        }
        public ActionResult Message()
        {
            return View();
        }
        public ActionResult Calendar()
        {
            return View();
        }
        public ActionResult NewEvent()
        {
            return View();
        }

        public ActionResult Account()
        {
            return View();
        }

        public ActionResult BillPay()
        {
            return View();
        }

    }
}