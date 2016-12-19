using Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class CommentController : Controller
    {
        // GET: Comment
        public ActionResult Index()
        {
            return View();
        }

        //GET Publication/ReadPost
        [Authorize]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .First();

                var comment = new Comment();
                comment.ArticleId = article.Id;

                return View(comment);
            }
        }

        //POST Publication/ReadPost

        [HttpPost]
        [Authorize]
        public ActionResult CreateComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var authorId = database
                        .Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;
                    comment.AuthorId = authorId;

                    database.Comments.Add(comment);
                    database.SaveChanges();
                    return RedirectToAction("Details", "Article", new { id = comment.ArticleId });
                }
            }

            return View(comment);
        }
    }
}