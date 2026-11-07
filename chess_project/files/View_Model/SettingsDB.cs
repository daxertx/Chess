using chess_project.files.Model;
using System;
using System.Data;

namespace chess_project.files.View_Model
{
    public class SettingsDB : BaseDB
    {
        public SettingsDB() : base()
        {
        }

        public Settings CreateDefaultSettings()
        {
            int rows = base.ExecuteNonQuery(
                "INSERT INTO settings (screen_size, white_piece_style, black_piece_style) VALUES (?, ?, ?)",
                (int)ScreenSize.Medium, "default", "default");

            if (rows < 1)
                return new Settings(0, ScreenSize.Medium, "default", "default");

            object max = base.GetScalar("SELECT MAX(id) FROM settings");
            int id = 0;
            if (max != null)
                id = Convert.ToInt32(max);

            return new Settings(id, ScreenSize.Medium, "default", "default");
        }

        public Settings GetSettingsById(int id)
        {
            DataTable dt = base.GetTable("SELECT id, screen_size, white_piece_style, black_piece_style FROM settings WHERE id = ?", id);
            if (dt == null || dt.Rows.Count == 0)
                return new Settings(id, ScreenSize.Medium, "default", "default");

            DataRow row = dt.Rows[0];
            ScreenSize size = (ScreenSize)ReadInt(row, "screen_size");
            string whiteStyle = row["white_piece_style"].ToString();
            string blackStyle = row["black_piece_style"].ToString();

            return new Settings(id, size, whiteStyle, blackStyle);
        }
    }
}
