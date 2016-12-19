﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class Article
    {
         private ICollection<Tag> tags;
         
         public Article()
        {
            this.tags = new HashSet<Tag>();
        }
        public Article (string authorId,string title,string context,int categoryId)
        {
            this.AuthorId = AuthorId;
            this.Title = title;
            this.Content = Content;
            this.CategoryId = categoryId;
            this.tags = new HashSet<Tag>();
        }

        public virtual ICollection<Tag> Tags
        {
            get { return this.tags;  }
            set { this.tags = value; }
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        
        public string Title { get; set; }

        [Required]
        
        public string Content { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public bool isAuthor(string name)
        {
            return this.Author.UserName.Equals(name);
        }

        public ICollection<Comment> Comments { get; set; }
    }
}