using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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
    public class AboutsController : ControllerBase
    {
        private readonly TWWContext _context;

        public AboutsController(TWWContext context)
        {
            _context = context;
        }

        // GET: api/Abouts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<About>>> GetAbouts()
        {
            if (_context.Abouts == null)
            {
                return NotFound();
            }
            return await _context.Abouts.ToListAsync();
        }

        // GET: api/Abouts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<About>> GetAbout(int id)
        {
            if (_context.Abouts == null)
            {
                return NotFound();
            }
            var about = await _context.Abouts.FindAsync(id);

            if (about == null)
            {
                return NotFound();
            }

            return about;
        }

        // PUT: api/Abouts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAbout(int id, About about)
        {
            if (id != about.Id)
            {
                return BadRequest();
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
            about.Content = sanitizer.Sanitize(about.Content);

            _context.Entry(about).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AboutExists(id))
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

        // POST: api/Abouts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<About>> PostAbout(About about)
        {
            if (_context.Abouts == null)
            {
                return Problem("Entity set 'TWWContext.Abouts'  is null.");
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
            about.Content = sanitizer.Sanitize(about.Content);

            _context.Abouts.Add(about);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAbout", new { id = about.Id }, about);
        }

        // DELETE: api/Abouts/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAbout(int id)
        {
            if (_context.Abouts == null)
            {
                return NotFound();
            }
            var about = await _context.Abouts.FindAsync(id);
            if (about == null)
            {
                return NotFound();
            }

            _context.Abouts.Remove(about);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AboutExists(int id)
        {
            return (_context.Abouts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
