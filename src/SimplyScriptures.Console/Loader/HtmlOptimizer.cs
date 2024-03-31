namespace SimplyScriptures.Console.Loader;

public class HtmlOptimizer
{
    #region Public Methods

    public static async Task OptimizeHtmlFilesAsync()
    {
        var files = Directory.GetFiles(@"C:\Source\SimplyScriptures\SimplyScriptures\SimplyScriptures\wwwroot\Scriptures");

        foreach (var file in files)
        {
            var text = await File.ReadAllTextAsync(file)
                .ConfigureAwait(false);

            var index = 0;
            while (true)
            {
                index = text.IndexOf(@"""></", index, StringComparison.OrdinalIgnoreCase);

                if (index == -1)
                {
                    break;
                }

                var start = index;
                while (text[start] != '<')
                {
                    start--;
                }

                var end = text.IndexOf('>', index + 3);

                var subText = text.Substring(start, end - start + 1);
                if (subText.Contains("style"))
                {
                    index++;
                    continue;
                }

                text = text.Replace(subText, "");
            }

            await File.WriteAllTextAsync(file, text)
                .ConfigureAwait(false);
        }
    }

    #endregion
}
