using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TWWAPI.Models;

namespace TWWAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController : ControllerBase
    {
        private readonly TWWContext _context;

        public BlogsController(TWWContext context)
        {
            _context = context;
        }

        // GET: api/Blogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Blog>>> GetBlogs()
        {
            if (_context.Blogs == null)
            {
                return NotFound();
            }
            return await _context.Blogs.Include(b => b.Category).ToListAsync();
        }

        // GET: api/Blogs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Blog>> GetBlog(int id)
        {
            if (_context.Blogs == null)
            {
                return NotFound();
            }
            var blog = await _context.Blogs.Include(b => b.Category).SingleOrDefaultAsync(b => b.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return blog;
        }

        // PUT: api/Blogs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBlog(int id, Blog blog)
        {
            if (id != blog.Id)
            {
                return BadRequest();
            }

            // Check if the image property is not null
            if (blog.Image != null)
            {
                // Split the image property into type and base64 value components
                string[] imageComponents = blog.Image.Split(',');

                // Check that there are two components (type and base64 value)
                if (imageComponents.Length != 2)
                {
                    return BadRequest("Image property is not a valid base64 value");
                }

                // Get the base64 value component and attempt to convert it to a byte array
                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(imageComponents[1]);
                }
                catch (FormatException)
                {
                    return BadRequest("Image property is not a valid base64 value");
                }
            }
            else
            {
                // Retrieve the blog post from the database and use its image value
                var existingBlog = _context.Blogs.Find(blog.Id);
                if (existingBlog != null)
                {
                    blog.Image = existingBlog.Image;
                }
            }


            // Sanitize HTML content
            // Define a whitelist of allowed HTML tags and attributes
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("h1");
            sanitizer.AllowedTags.Add("h2");
            sanitizer.AllowedTags.Add("h3");
            sanitizer.AllowedTags.Add("h4");
            sanitizer.AllowedTags.Add("h5");
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("ol");
            sanitizer.AllowedTags.Add("li");
            sanitizer.AllowedTags.Add("span");
            sanitizer.AllowedAttributes.Add("style");

            // Sanitize HTML content using the whitelist
            blog.Content = sanitizer.Sanitize(blog.Content);

            _context.Entry(blog).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Blogs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Blog>> PostBlog(Blog blog)
        {
            if (_context.Blogs == null)
            {
                return Problem("Entity set 'TWWContext.Blogs' is null.");
            }

            // Check if the image property is not null
            if (blog.Image != null)
            {
                // Split the image property into type and base64 value components
                string[] imageComponents = blog.Image.Split(',');

                // Check that there are two components (type and base64 value)
                if (imageComponents.Length != 2)
                {
                    return BadRequest("Image property is not a valid base64 value");
                }

                // Get the base64 value component and attempt to convert it to a byte array
                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(imageComponents[1]);
                }
                catch (FormatException)
                {
                    return BadRequest("Image property is not a valid base64 value");
                }
            }

            // Sanitize HTML content
            // Define a whitelist of allowed HTML tags and attributes
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("h1");
            sanitizer.AllowedTags.Add("h2");
            sanitizer.AllowedTags.Add("h3");
            sanitizer.AllowedTags.Add("h4");
            sanitizer.AllowedTags.Add("h5");
            sanitizer.AllowedTags.Add("p");
            sanitizer.AllowedTags.Add("ul");
            sanitizer.AllowedTags.Add("ol");
            sanitizer.AllowedTags.Add("li");
            sanitizer.AllowedTags.Add("span");
            sanitizer.AllowedAttributes.Add("style");

            // Sanitize HTML content using the whitelist
            blog.Content = sanitizer.Sanitize(blog.Content);

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBlog", new { id = blog.Id }, blog);
        }


        // DELETE: api/Blogs/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            if (_context.Blogs == null)
            {
                return NotFound();
            }
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlogExists(int id)
        {
            return (_context.Blogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
