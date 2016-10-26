using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using SecureGameCockNetwork.Models;

namespace SecureGameCockNetwork.Controllers
{
    public class PostsController : ApiController
    {
        private MessagingBoardContext _ctx;
        public PostsController()
        {
            this._ctx = new MessagingBoardContext();
        }

        // GET api/<controller>
        public IEnumerable<Post> Get()
        {
            var allPosts = this._ctx.Posts.OrderByDescending(x => x.DatePosted).ToList();
            
            //foreach (var post in allPosts)
            //{
            //    var allCommentsOnThisPost = _ctx.Comments.Where(p => p.ParentPost.Id == post.Id).ToList();
            //    foreach (var comment in allCommentsOnThisPost)
            //    {
            //        post.Comments.Add(comment);
            //    }
            //}
            return allPosts;
        }
    }
}
