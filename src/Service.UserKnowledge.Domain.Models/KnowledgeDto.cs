﻿using Service.Core.Domain.Models.Education;

namespace Service.UserKnowledge.Domain.Models
{
	public class KnowledgeDto
	{
		public EducationTutorial Tutorial { get; set; }

		public string[] Ids { get; set; }
	}
}