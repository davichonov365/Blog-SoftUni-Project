using Blog.Models;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System;

namespace Blog.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var articles = database.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Tags)
                    .ToList();

                return View(articles);
            }
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();

                if (article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
            }
        }

        // GET: Article/Create
        [Authorize]
        public ActionResult Create()
        {
            using (var database = new BlogDbContext())
            {
                var model = new ArticleViewModel();
                model.Categories = database.Categories
                                  .OrderBy(c => c.Name)
                                  .ToList();

                return View(model);
            }
                
        }

        // POST: Article/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(ArticleViewModel model)
        {
            
             using (var db = new BlogDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name));

                    var article = new Article ();
                            article.AuthorId = user.Id;
                            article.Title = model.Title;
                            article.Content = model.Content;
                            article.CategoryId = model.CategoryId;
                            

                    this.SetArticlesTags(article, model, db);

                    db.Articles.Add(article);
                    db.SaveChanges();

                    return RedirectToAction("Index");
               }

            
        }
        

        //Get:Aricle/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                //Get Article from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .Include(a => a.Tags)
                    .First();

                if (! IsUserAutorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                //Check if article exists
                if (article == null)
                {
                    return HttpNotFound();
                }
                //Pass article to view
                return View(article);
            }
        }
        //Post Article/delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if(id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();
            //Check if article exists
            if (article == null)
            {
                return HttpNotFound();
            }
                //Remove Article from db
                database.Articles.Remove(article);
                database.SaveChanges();
                //Redirect to Index Page
                return RedirectToAction("Index");
            }
            

        }
        //Get:Article Edit
        [Authorize]
        public ActionResult Edit (int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();


                if (!IsUserAutorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (article == null)
                {
                    return HttpNotFound();
                }
                //Create the view model
                var model = new ArticleViewModel();
                model.Id = article.Id;
                model.Title = article.Title;
                model.Content = article.Content;
                model.CategoryId = article.CategoryId;
                model.Categories = database.Categories
                    .OrderBy(c => c.Name)
                    .ToList();

                model.Tags = string.Join(",", article.Tags.Select(t => t.Name));

                //Pass The view model to view
                return View(model);
            }
          }

        //Post:Article Edit
        [HttpPost]
        [Authorize]
        public ActionResult Edit(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var article = database.Articles
                    .FirstOrDefault(a => a.Id == model.Id);

                    //Set article properties
                    article.Title = model.Title;
                    article.Content = model.Content;
                    article.CategoryId = model.CategoryId;
                    this.SetArticlesTags(article, model, database);

                    //Save article in database
                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
                       return View(model);
         }

        private void SetArticlesTags(Article article, ArticleViewModel model, BlogDbContext database)
        {
            //Split Tags
            var tagsStrings = model.Tags
               .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
               .Select(t => t.ToLower())
               .Distinct();
            //Clear all current article tags
            article.Tags.Clear();
            //Set new article tags
            foreach (var tagString in tagsStrings)
            {
                //Get tag from db by its name 
                Tag tag = database.Tags.FirstOrDefault(t => t.Name
                         .Equals(tagString));
                //If the tag is null,create new tag
                if (tag == null)
                {
                    tag = new Tag() { Name = tagString };
                    database.Tags.Add(tag);
                }
                //Add tag to article tags
                article.Tags.Add(tag);
            }
            
            
        }

        private bool IsUserAutorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.isAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }

        }
    }