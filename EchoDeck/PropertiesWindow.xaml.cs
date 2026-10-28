using System.Diagnostics;
using System.Windows;
using TagLib;
namespace EchoDeck;
public partial class PropertiesWindow : Window
{
    private readonly string _path;
    public PropertiesWindow(string path)
    {
        InitializeComponent();
        WindowHelper.AttachChrome(this);
        _path = path;
        LoadProperties();
    }

    private void LoadProperties()
    {
        try
        {
            var info = new FileInfo(_path);
            PropType.Text = info.Extension.ToUpperInvariant().TrimStart('.');
            PropLocation.Text = info.FullName;
            
            try
            {
                using var tag = TagLib.File.Create(_path);
                PropTitle.Text = string.IsNullOrWhiteSpace(tag.Tag.Title) ? info.Name : tag.Tag.Title;
                PropArtist.Text = tag.Tag.Performers.Length > 0 ? string.Join(", ", tag.Tag.Performers) : "Unknown Artist";
                PropAlbum.Text = string.IsNullOrWhiteSpace(tag.Tag.Album) ? "Unknown Album" : tag.Tag.Album;
                PropGenre.Text = tag.Tag.Genres.Length > 0 ? string.Join(", ", tag.Tag.Genres) : "Unknown Genre";
                PropYear.Text = tag.Tag.Year.ToString();
                PropLength.Text = tag.Properties.Duration.ToString(@"hh\:mm\:ss");
             if (tag.Properties.MediaTypes.HasFlag(TagLib.MediaTypes.Video))
                {
                    PropResolution.Text = $"{tag.Properties.VideoWidth} × {tag.Properties.VideoHeight}";
                }
                else
                {
                    PropResolution.Text = "Audio only";
                }
            }
            catch
            {
                PropTitle.Text = info.Name;
                PropArtist.Text = "Unknown";
                PropAlbum.Text = "Unknown";
                PropGenre.Text = "Unknown";
                PropYear.Text = "Unknown";
                PropLength.Text = "Unknown";
                PropResolution.Text = "Unknown";
            }
        }
        catch { }
    }

    private void OpenLocation_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{_path}\"") { UseShellExecute = true }); }
        catch { }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}