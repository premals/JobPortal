using JobSeekerService.Application.Interfaces;
using JobSeekerService.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JobSeekerService.Infrastructure.Resume
{
    public class ResumePdfGenerator : IResumePdfGenerator
    {
        public byte[] Generate(ResumeDraft draft)
        {
            if (draft == null)
                throw new ArgumentNullException(nameof(draft));

            QuestPDF.Settings.License = LicenseType.Community;
            var theme = ResumeTheme.FromTemplate(draft.Template);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    page.Content().Column(col =>
                    {
                        col.Item().Element(c => BuildHeader(c, draft, theme));
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(theme.Divider);

                        if (theme.Layout == ResumeLayout.Classic)
                        {
                            BuildClassic(col, draft, theme);
                        }
                        else
                        {
                            BuildModern(col, draft, theme);
                        }
                    });
                });
            }).GeneratePdf();
        }

        private static void BuildHeader(IContainer container, ResumeDraft draft, ResumeTheme theme)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(draft.FullName ?? "Resume").FontSize(22).SemiBold().FontColor(theme.Primary);
                    if (!string.IsNullOrWhiteSpace(draft.Headline))
                        col.Item().Text(draft.Headline).FontSize(12).FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(200).Column(col =>
                {
                    foreach (var item in BuildContactItems(draft))
                    {
                        col.Item().AlignRight().Text(item).FontSize(9).FontColor(Colors.Grey.Darken2);
                    }
                });
            });
        }

        private static void BuildClassic(ColumnDescriptor col, ResumeDraft draft, ResumeTheme theme)
        {
            AddSection(col, "Summary", draft.Summary, theme, () => draft.Summary);
            AddExperience(col, draft, theme);
            AddEducation(col, draft, theme);
            AddProjects(col, draft, theme);
            AddSkillList(col, "Skills", draft.Skills, theme);
            AddSimpleList(col, "Certifications", draft.Certifications.Select(c => $"{c.Name} · {c.Issuer} ({c.Year})").ToList(), theme);
            AddSimpleList(col, "Languages", draft.Languages.Select(l => $"{l.Name} · {l.Proficiency}").ToList(), theme);
        }

        private static void BuildModern(ColumnDescriptor col, ResumeDraft draft, ResumeTheme theme)
        {
            col.Item().Row(row =>
            {
                row.RelativeItem(2).Column(main =>
                {
                    AddSection(main, "Summary", draft.Summary, theme, () => draft.Summary);
                    AddExperience(main, draft, theme);
                    AddEducation(main, draft, theme);
                    AddProjects(main, draft, theme);
                });

                row.RelativeItem(1).Column(side =>
                {
                    AddSkillList(side, "Skills", draft.Skills, theme);
                    AddSimpleList(side, "Certifications", draft.Certifications.Select(c => $"{c.Name} · {c.Issuer}").ToList(), theme);
                    AddSimpleList(side, "Languages", draft.Languages.Select(l => $"{l.Name} · {l.Proficiency}").ToList(), theme);
                });
            });
        }

        private static void AddSection(ColumnDescriptor col, string title, string? content, ResumeTheme theme, Func<string?> predicate)
        {
            if (string.IsNullOrWhiteSpace(predicate()))
                return;

            col.Item().PaddingBottom(6).Text(title).FontSize(11).SemiBold().FontColor(theme.Primary);
            col.Item().Text(content ?? string.Empty).FontSize(10).LineHeight(1.4f);
            col.Item().PaddingBottom(10);
        }

        private static void AddExperience(ColumnDescriptor col, ResumeDraft draft, ResumeTheme theme)
        {
            if (draft.WorkHistory == null || draft.WorkHistory.Count == 0)
                return;

            col.Item().PaddingBottom(6).Text("Experience").FontSize(11).SemiBold().FontColor(theme.Primary);
            foreach (var work in draft.WorkHistory)
            {
                col.Item().Text($"{work.Role} · {work.Company}").FontSize(10).SemiBold();
                col.Item().Text($"{work.StartDate} - {work.EndDate}").FontSize(9).FontColor(Colors.Grey.Darken2);
                if (!string.IsNullOrWhiteSpace(work.Description))
                    col.Item().Text(work.Description).FontSize(9).LineHeight(1.4f);
                col.Item().PaddingBottom(8);
            }
            col.Item().PaddingBottom(6);
        }

        private static void AddEducation(ColumnDescriptor col, ResumeDraft draft, ResumeTheme theme)
        {
            if (draft.EducationHistory == null || draft.EducationHistory.Count == 0)
                return;

            col.Item().PaddingBottom(6).Text("Education").FontSize(11).SemiBold().FontColor(theme.Primary);
            foreach (var edu in draft.EducationHistory)
            {
                col.Item().Text($"{edu.Degree} · {edu.Field}").FontSize(10).SemiBold();
                col.Item().Text($"{edu.School} · {edu.GraduationYear}").FontSize(9).FontColor(Colors.Grey.Darken2);
                col.Item().PaddingBottom(6);
            }
        }

        private static void AddProjects(ColumnDescriptor col, ResumeDraft draft, ResumeTheme theme)
        {
            if (draft.Projects == null || draft.Projects.Count == 0)
                return;

            col.Item().PaddingBottom(6).Text("Projects").FontSize(11).SemiBold().FontColor(theme.Primary);
            foreach (var project in draft.Projects)
            {
                col.Item().Text(project.Name).FontSize(10).SemiBold();
                if (!string.IsNullOrWhiteSpace(project.Role))
                    col.Item().Text(project.Role).FontSize(9).FontColor(Colors.Grey.Darken2);
                if (!string.IsNullOrWhiteSpace(project.Description))
                    col.Item().Text(project.Description).FontSize(9).LineHeight(1.4f);
                col.Item().PaddingBottom(6);
            }
        }

        private static void AddSkillList(ColumnDescriptor col, string title, List<string>? skills, ResumeTheme theme)
        {
            if (skills == null || skills.Count == 0)
                return;

            col.Item().PaddingBottom(6).Text(title).FontSize(11).SemiBold().FontColor(theme.Primary);
            col.Item().Text(string.Join(" · ", skills)).FontSize(9).LineHeight(1.4f);
            col.Item().PaddingBottom(10);
        }

        private static void AddSimpleList(ColumnDescriptor col, string title, List<string> items, ResumeTheme theme)
        {
            if (items == null || items.Count == 0)
                return;

            col.Item().PaddingBottom(6).Text(title).FontSize(11).SemiBold().FontColor(theme.Primary);
            foreach (var item in items.Where(text => !string.IsNullOrWhiteSpace(text)))
            {
                col.Item().Text(item).FontSize(9);
            }
            col.Item().PaddingBottom(10);
        }

        private static IEnumerable<string> BuildContactItems(ResumeDraft draft)
        {
            if (!string.IsNullOrWhiteSpace(draft.Email))
                yield return draft.Email;
            if (!string.IsNullOrWhiteSpace(draft.Phone))
                yield return draft.Phone;
            if (!string.IsNullOrWhiteSpace(draft.Location))
                yield return draft.Location;
        }

        private record ResumeTheme(string Primary, string Divider, ResumeLayout Layout)
        {
            public static ResumeTheme FromTemplate(string? template)
            {
                var normalized = (template ?? string.Empty).Trim().ToLowerInvariant();
                return normalized switch
                {
                    "classic" => new ResumeTheme(Colors.Grey.Darken4, Colors.Grey.Lighten2, ResumeLayout.Classic),
                    "creative" => new ResumeTheme(Colors.Teal.Darken2, Colors.Teal.Lighten4, ResumeLayout.Modern),
                    _ => new ResumeTheme(Colors.Blue.Darken2, Colors.Blue.Lighten4, ResumeLayout.Modern)
                };
            }
        }

        private enum ResumeLayout
        {
            Classic,
            Modern
        }
    }
}
