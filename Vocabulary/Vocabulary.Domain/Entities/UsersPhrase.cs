using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary.Domain.Entities
{
    public class UsersPhrase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal Id { get; set; }
        public decimal UserId { get; set; }
        public decimal? GlobalPhraseId { get; set; }
        public decimal LanguageId { get; set; }
        public decimal? GlossaryId { get; set; }
        public string GlossaryName { get; set; }
        public string Phrase { get; set; }
        public PhraseType PhraseType { get; set; }
        public string Transcription { get; set; }
        public int? Frequency { get; set; }
       
        [Range(0.0, 1.0)] // 0 - unknown, 1 - learned
        public double LearningState { get; set; }
        public byte[] Audio { get; set; }

        public UsersPhrase()
        {
            Id = 0;
            UserId = 0;
            Phrase = "";
            PhraseType = PhraseType.Phrase;
            Audio = null;
            LanguageId = 1;
            Transcription = null;
            Frequency = null;
            LearningState = 0;
            GlossaryName = null;
            GlossaryId = null;
        }
        public UsersPhrase(UsersPhrase p)
        {
            Id = p.Id;
            UserId = p.UserId;
            Phrase = p.Phrase;
            PhraseType = p.PhraseType;
            Audio = p.Audio;
            LanguageId = p.LanguageId;
            Transcription = p.Transcription;
            Frequency = p.Frequency;
            LearningState = p.LearningState;
            GlossaryName = p.GlossaryName;
            GlossaryId = p.GlossaryId;
        }
        public UsersPhrase(GlobalPhrase p)
        {
            Phrase = p.Phrase;
            PhraseType = p.PhraseType;
            Audio = p.Audio;
            LanguageId = p.LanguageId;
            Transcription = p.Transcription;
            Frequency = p.Frequency;
            LearningState = 0;
            GlossaryName = "";
            GlossaryId = null;
        }
    }
}
