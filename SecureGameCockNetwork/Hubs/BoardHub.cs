using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using SecureGameCockNetwork.Models;
using SecureGameCockNetwork.Providers;

namespace SecureGameCockNetwork.Hubs
{
    public class BoardHub : Hub
    {
        public void WritePost(string username, string message)
        {
            var ctx = new MessagingBoardContext();
            var post = new Post { Message = EncryptDecryptProvider.Encrypt(message), Username = username, DatePosted = DateTime.Now };
            ctx.Posts.Add(post);
            ctx.SaveChanges();

            Clients.All.receivedNewPost(post.Id, post.Username, EncryptDecryptProvider.Decrypt(post.Message), post.DatePosted);
        }

        public void AddComment(int postId, string comment, string username)
        {
            var ctx = new MessagingBoardContext();
            var post = ctx.Posts.FirstOrDefault(p => p.Id == postId);

            if (post != null)
            {
                var newComment = new Comment { ParentPost = post, Message = EncryptDecryptProvider.Encrypt(comment), Username = username, DatePosted = DateTime.Now };
                ctx.Comments.Add(newComment);
                ctx.SaveChanges();

                Clients.All.receivedNewComment(newComment.ParentPost.Id, newComment.Id, EncryptDecryptProvider.Decrypt(newComment.Message), newComment.Username, newComment.DatePosted);
            }
        }
    }
}