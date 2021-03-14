using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Task2.Models;

namespace Task2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class notesController : ControllerBase
    {
        private static int currentId = 0;
        private const int N = 5;
        private static Dictionary<int, Note> repository = new Dictionary<int, Note>();

        // GET: <notesController>/?query=string
        [HttpGet]
        public JsonResult Get(string query = null)
        {
            var data = new List<Note>();

            foreach (var pair in repository)
            {
                var note = pair.Value;
                if (Check(note, query))
                {
                    data.Add(Convert(note));
                }
            }

            return new JsonResult(data);
        }

        // GET <notesController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (!repository.ContainsKey(id))
            {
                return NotFound();
            }

            var note = repository[id];

            var data = Convert(note);

            return new JsonResult(data);
        }

        // POST <notesController>
        [HttpPost]
        public IActionResult Post([FromBody] NoteJsonModel model)
        { 
            if (model.Content == null)
            {
                return StatusCode(500);
            }

            var note = CreateOrUpdate(currentId++, model);

            var data = Convert(note);

            return new JsonResult(data);
        }

        // PUT <notesController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] NoteJsonModel model)
        {
            if (!repository.ContainsKey(id))
            {
                return NotFound();
            }

            CreateOrUpdate(id, model);

            return StatusCode(202);
        }

        // DELETE <notesController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!repository.ContainsKey(id))
            {
                return NotFound();
            }

            repository.Remove(id);

            return StatusCode(202);
        }

        private Note CreateOrUpdate(int id, NoteJsonModel model)
        {
            var note = repository.ContainsKey(id)
                ? repository[id]
                : new Note { Id = id };

            note.Title = model.Title ?? note.Title;
            note.Content = model.Content ?? note.Content;

            if (repository.ContainsKey(id))
            {
                repository.Remove(id);
            }

            repository.Add(id, note);

            return note;
        }

        private Note Convert(Note note)
        {
            var model = new Note
            {
                Id = note.Id,
                Title = note.Title ?? note.Content.Substring(0, Math.Min(N, note.Content.Length)),
                Content = note.Content
            };

            return model;
        }

        private bool Check(Note note, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }

            if (note.Content.Contains(query))
            {
                return true;
            }

            if (note.Title != null && note.Title.Contains(query))
            {
                return true;
            }

            return false;
        }
    }
}
