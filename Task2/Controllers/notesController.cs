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

        /// <summary>
        /// Получить заметки, удовлетворяющие запросу
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /notes?query=string
        /// Если запрос пустой, то будут возвращены все заметки
        /// </remarks>
        /// <param name="query">Строка для поиска</param>
        /// <response code="200">Возвращает найденные заметки</response>
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

        /// <summary>
        /// Получить заметку
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /notes/1
        ///
        /// </remarks>
        /// <param name="id">Идентификатор заметки</param>
        /// <response code="200">Возвращает найденную заметку</response>
        /// <response code="404">Если заметка не найдена</response> 
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

        /// <summary>
        /// Добавить заметку
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /notes
        ///
        /// </remarks>
        /// <response code="200">Возвращает созданную заметку</response>
        /// <response code="500">Передана заметка без содержания</response> 
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

        /// <summary>
        /// Обновить заметку
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /notes/1
        ///
        /// </remarks>
        /// <param name="id">Идентификатор заметки</param>
        /// <response code="200">Если заметка успешно обновлена</response>
        /// <response code="404">Если заметка не найдена</response>
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] NoteJsonModel model)
        {
            if (!repository.ContainsKey(id))
            {
                return NotFound();
            }

            CreateOrUpdate(id, model);

            return StatusCode(200);
        }

        /// <summary>
        /// Удалить заметку
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /notes/1
        ///
        /// </remarks>
        /// <param name="id">Идентификатор заметки</param>
        /// <response code="200">Если заметка успешно удалена</response>
        /// <response code="404">Если заметка не найдена</response>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!repository.ContainsKey(id))
            {
                return NotFound();
            }

            repository.Remove(id);

            return StatusCode(200);
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
