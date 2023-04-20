using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TWWAPI.Models;

namespace TWWAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly TWWContext _context;

        public ProfilesController(TWWContext context)
        {
            _context = context;
        }

        // GET: api/Profiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
            if (_context.Profiles == null)
            {
                return NotFound();
            }
            return await _context.Profiles.ToListAsync();
        }

        // GET: api/Profiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Profile>> GetProfile(int id)
        {
            if (_context.Profiles == null)
            {
                return NotFound();
            }
            var profile = await _context.Profiles.FindAsync(id);

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        // PUT: api/Profiles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfile(int id, Profile profile)
        {
            if (id != profile.Id)
            {
                return BadRequest();
            }

            // Check if the image property is not null
            if (profile.Image != null)
            {
                // Split the image property into type and base64 value components
                string[] imageComponents = profile.Image.Split(',');

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
                var existingProfile = _context.Blogs.Find(profile.Id);
                if (existingProfile != null)
                {
                    profile.Image = existingProfile.Image;
                }
            }

            _context.Entry(profile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(id))
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

        // POST: api/Profiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Profile>> PostProfile(Profile profile)
        {
            if (_context.Profiles == null)
            {
                return Problem("Entity set 'TWWContext.Profiles'  is null.");
            }

            // Check if the image property is not null
            if (profile.Image != null)
            {
                // Split the image property into type and base64 value components
                string[] imageComponents = profile.Image.Split(',');

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

            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProfile", new { id = profile.Id }, profile);
        }

        // DELETE: api/Profiles/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(int id)
        {
            if (_context.Profiles == null)
            {
                return NotFound();
            }
            var profile = await _context.Profiles.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfileExists(int id)
        {
            return (_context.Profiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
