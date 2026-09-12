using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Talkift.Client.Views
{
    public sealed partial class EmojiPicker : UserControl
    {
        private static readonly List<(string Emoji, string Name)> AllEmojis = new()
        {
            ("\U0001F600", "grinning"), ("\U0001F601", "beaming"), ("\U0001F602", "joy"),
            ("\U0001F603", "smiley"), ("\U0001F604", "smile"), ("\U0001F605", "sweat"),
            ("\U0001F606", "laughing"), ("\U0001F607", "innocent"), ("\U0001F608", "smiling"),
            ("\U0001F609", "wink"), ("\U0001F60A", "blush"), ("\U0001F60B", "yum"),
            ("\U0001F60C", "relieved"), ("\U0001F60D", "heart_eyes"), ("\U0001F60E", "sunglasses"),
            ("\U0001F60F", "smirk"), ("\U0001F612", "unamused"), ("\U0001F613", "sweat"),
            ("\U0001F614", "pensive"), ("\U0001F615", "confused"), ("\U0001F616", "confounded"),
            ("\U0001F618", "kissing_heart"), ("\U0001F61C", "stuck_out_tongue_winking_eye"),
            ("\U0001F61D", "stuck_out_tongue_closed_eyes"), ("\U0001F61E", "disappointed"),
            ("\U0001F61F", "worried"), ("\U0001F620", "angry"), ("\U0001F621", "rage"),
            ("\U0001F622", "cry"), ("\U0001F623", "persevere"), ("\U0001F624", "triumph"),
            ("\U0001F625", "disappointed_relieved"), ("\U0001F628", "fearful"), ("\U0001F629", "weary"),
            ("\U0001F62A", "sleepy"), ("\U0001F62B", "tired"), ("\U0001F62D", "sob"),
            ("\U0001F630", "cold_sweat"), ("\U0001F631", "scream"), ("\U0001F632", "astonished"),
            ("\U0001F633", "flushed"), ("\U0001F634", "sleeping"), ("\U0001F635", "dizzy"),
            ("\U0001F637", "mask"), ("\U0001F44D", "thumbs_up"), ("\U0001F44E", "thumbs_down"),
            ("\U0001F44F", "clap"), ("\U0001F450", "open_hands"), ("\U0001F4AA", "muscle"),
            ("\U0001F44C", "ok_hand"), ("\U0001F44B", "wave"), ("\U0001F44A", "punch"),
            ("\U0001F449", "point_right"), ("\U0001F448", "point_left"), ("\U0001F446", "point_up"),
            ("\U0001F595", "middle_finger"), ("\U0001F590", "hand"), ("\U0000270A", "fist"),
            ("\U0000270C", "v"), ("\U0001F440", "eyes"), ("\U0001F4A4", "zzz"),
            ("\U0001F4A3", "boom"), ("\U0001F4A5", "collision"), ("\U0001F4A9", "poop"),
            ("\U0001F4AB", "dizzy"), ("\U0001F4AC", "speech_balloon"), ("\U0001F4AF", "100"),
            ("\U0001F4B0", "moneybag"), ("\U0001F380", "ribbon"), ("\U0001F381", "gift"),
            ("\U0001F382", "birthday"), ("\U0001F383", "jack_o_lantern"), ("\U0001F384", "christmas"),
            ("\U0001F385", "santa"), ("\U0001F386", "fireworks"), ("\U0001F387", "sparkler"),
            ("\U0001F388", "balloon"), ("\U0001F389", "tada"), ("\U0001F38A", "confetti"),
            ("\U0001F525", "fire"), ("\U0001F4A1", "bulb"), ("\U0001F4A2", "anger"),
            ("\U0001F4A6", "sweat_drops"), ("\U0001F4A7", "droplet"), ("\U0001F4A8", "dash"),
            ("\U00002764", "heart"), ("\U0001F494", "broken_heart"), ("\U0001F495", "two_hearts"),
            ("\U0001F496", "sparkling_heart"), ("\U0001F497", "heartpulse"),
            ("\U0001F498", "cupid"), ("\U0001F499", "blue_heart"), ("\U0001F49A", "green_heart"),
            ("\U0001F49B", "yellow_heart"), ("\U0001F49C", "purple_heart"),
            ("\U00002B50", "star"), ("\U0001F31F", "star2"), ("\U00002600", "sunny"),
            ("\U00002601", "cloud"), ("\U000026C5", "partly_sunny"), ("\U00002728", "sparkles"),
            ("\U0001F300", "cyclone"), ("\U0001F308", "rainbow"), ("\U0001F30A", "ocean"),
            ("\U0001F30D", "earth_africa"), ("\U0001F30E", "earth_americas"),
            ("\U0001F30F", "earth_asia"), ("\U0001F330", "chestnut"),
            ("\U0001F331", "seedling"), ("\U0001F332", "evergreen_tree"),
            ("\U0001F333", "deciduous_tree"), ("\U0001F334", "palm_tree"),
            ("\U0001F335", "cactus"), ("\U0001F337", "tulip"), ("\U0001F338", "cherry_blossom"),
            ("\U0001F339", "rose"), ("\U0001F33A", "hibiscus"), ("\U0001F33B", "sunflower"),
            ("\U0001F33C", "blossom"), ("\U0001F33D", "corn"), ("\U0001F33E", "ear_of_rice"),
            ("\U0001F33F", "herb"), ("\U0001F340", "four_leaf_clover"),
            ("\U0001F341", "maple_leaf"), ("\U0001F342", "fallen_leaf"),
            ("\U0001F343", "leaves"), ("\U0001F344", "mushroom"), ("\U0001F345", "tomato"),
            ("\U0001F346", "eggplant"), ("\U0001F347", "grapes"), ("\U0001F348", "melon"),
            ("\U0001F349", "watermelon"), ("\U0001F34A", "tangerine"),
            ("\U0001F34B", "lemon"), ("\U0001F34C", "banana"), ("\U0001F34D", "pineapple"),
            ("\U0001F34E", "apple"), ("\U0001F34F", "green_apple"),
            ("\U0001F350", "pear"), ("\U0001F351", "peach"), ("\U0001F352", "cherries"),
            ("\U0001F353", "strawberry"), ("\U0001F354", "hamburger"),
            ("\U0001F355", "pizza"), ("\U0001F356", "meat_on_bone"),
            ("\U0001F357", "poultry_leg"), ("\U0001F358", "rice_cracker"),
            ("\U0001F359", "rice_ball"), ("\U0001F35A", "rice"), ("\U0001F35B", "curry"),
            ("\U0001F35C", "ramen"), ("\U0001F35D", "spaghetti"), ("\U0001F35E", "bread"),
            ("\U0001F35F", "fries"), ("\U0001F360", "sweet_potato"),
            ("\U0001F361", "dango"), ("\U0001F362", "oden"), ("\U0001F363", "sushi"),
            ("\U0001F366", "icecream"), ("\U0001F370", "cake"), ("\U0001F371", "bento"),
            ("\U0001F372", "stew"), ("\U0001F373", "egg"), ("\U0001F374", "fork_and_knife"),
            ("\U0001F375", "tea"), ("\U0001F376", "sake"), ("\U0001F377", "wine_glass"),
            ("\U0001F378", "cocktail"), ("\U0001F37A", "beer"), ("\U0001F37B", "beers"),
            ("\U0001F380", "ribbon"), ("\U0001F381", "gift"), ("\U0001F382", "birthday"),
            ("\U0001F3A0", "carousel_horse"), ("\U0001F3A1", "ferris_wheel"),
            ("\U0001F3A2", "roller_coaster"), ("\U0001F3A3", "fishing_pole_and_fish"),
            ("\U0001F3A4", "microphone"), ("\U0001F3A5", "movie_camera"),
            ("\U0001F3A7", "headphones"), ("\U0001F3A8", "art"),
            ("\U0001F3A9", "tophat"), ("\U0001F3AB", "ticket"),
            ("\U0001F3AC", "clapper"), ("\U0001F3AD", "performing_arts"),
            ("\U0001F3AE", "video_game"), ("\U0001F3AF", "dart"),
            ("\U0001F3B1", "8ball"), ("\U0001F3B3", "bowling"),
            ("\U0001F3B5", "musical_note"), ("\U0001F3B6", "notes"),
            ("\U0001F3B7", "saxophone"), ("\U0001F3B8", "guitar"),
            ("\U0001F3BA", "trumpet"), ("\U0001F3BE", "tennis"),
            ("\U0001F3BF", "ski"), ("\U0001F3C0", "basketball"),
            ("\U0001F3C6", "trophy"), ("\U0001F3C8", "football"),
            ("\U0001F3CA", "swimmer"), ("\U0001F3CB", "weight_lifter"),
            ("\U0001F3CC", "golfer"), ("\U0001F3CF", "cricket"),
            ("\U0001F3D1", "volleyball"), ("\U0001F3D3", "ping_pong"),
            ("\U0001F3D5", "tent"), ("\U0001F3D6", "fuji"),
            ("\U0001F3D8", "house"), ("\U0001F3D9", "cityscape"),
            ("\U0001F3DA", "classical_building"), ("\U0001F3DB", "museum"),
            ("\U0001F3DC", "desert"), ("\U0001F3DD", "island"),
            ("\U0001F3DE", "park"), ("\U0001F3DF", "stadium"),
            ("\U0001F3E0", "house"), ("\U0001F3E2", "office"),
            ("\U0001F3E3", "post_office"), ("\U0001F3E5", "hospital"),
            ("\U0001F3E6", "bank"), ("\U0001F3E7", "atm"),
            ("\U0001F3E8", "hotel"), ("\U0001F3E9", "love_hotel"),
            ("\U0001F3EA", "convenience_store"), ("\U0001F3EB", "school"),
            ("\U0001F3EC", "department_store"), ("\U0001F3ED", "factory"),
            ("\U0001F400", "rat"), ("\U0001F401", "mouse2"),
            ("\U0001F402", "ox"), ("\U0001F403", "water_buffalo"),
            ("\U0001F404", "cow2"), ("\U0001F405", "tiger2"),
            ("\U0001F406", "leopard"), ("\U0001F407", "rabbit2"),
            ("\U0001F408", "cat2"), ("\U0001F409", "dragon"),
            ("\U0001F40A", "crocodile"), ("\U0001F40B", "whale2"),
            ("\U0001F40C", "snail"), ("\U0001F40D", "snake"),
            ("\U0001F40E", "racehorse"), ("\U0001F40F", "ram"),
            ("\U0001F410", "goat"), ("\U0001F411", "sheep"),
            ("\U0001F412", "monkey"), ("\U0001F413", "rooster"),
            ("\U0001F414", "chicken"), ("\U0001F415", "dog2"),
            ("\U0001F416", "pig2"), ("\U0001F417", "boar"),
            ("\U0001F418", "elephant"), ("\U0001F419", "octopus"),
            ("\U0001F41A", "shell"), ("\U0001F41B", "bug"),
            ("\U0001F41C", "ant"), ("\U0001F41D", "bee"),
            ("\U0001F41E", "ladybug"), ("\U0001F41F", "fish"),
            ("\U0001F420", "tropical_fish"), ("\U0001F421", "blowfish"),
            ("\U0001F422", "turtle"), ("\U0001F423", "hatching_chick"),
            ("\U0001F424", "baby_chick"), ("\U0001F425", "hatched_chick"),
            ("\U0001F426", "bird"), ("\U0001F427", "penguin"),
            ("\U0001F428", "koala"), ("\U0001F429", "poodle"),
            ("\U0001F42B", "camel"), ("\U0001F42C", "dolphin"),
            ("\U0001F42D", "mouse"), ("\U0001F42E", "cow2"),
            ("\U0001F42F", "tiger"), ("\U0001F430", "rabbit"),
            ("\U0001F431", "cat"), ("\U0001F432", "dragon_face"),
            ("\U0001F433", "whale"), ("\U0001F434", "horse"),
            ("\U0001F435", "monkey_face"), ("\U0001F436", "dog"),
            ("\U0001F437", "pig"), ("\U0001F438", "frog"),
            ("\U0001F439", "hamster"), ("\U0001F43A", "wolf"),
            ("\U0001F43B", "bear"), ("\U0001F43C", "panda_face"),
            ("\U0001F43D", "pig_nose"), ("\U0001F43E", "feet"),
            ("\U0001F440", "eyes")
        };

        private readonly ObservableCollection<string> _displayedEmojis = new();

        public event EventHandler<string>? EmojiSelected;

        public EmojiPicker()
        {
            this.InitializeComponent();
            LoadEmojis(AllEmojis);
        }

        private void LoadEmojis(List<(string Emoji, string Name)> emojis)
        {
            _displayedEmojis.Clear();
            foreach (var (emoji, _) in emojis)
                _displayedEmojis.Add(emoji);
            EmojiGridView.ItemsSource = _displayedEmojis;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(query))
            {
                LoadEmojis(AllEmojis);
            }
            else
            {
                var filtered = AllEmojis.FindAll(x => x.Name.Contains(query));
                LoadEmojis(filtered);
            }
        }

        private void EmojiGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is string emoji)
            {
                EmojiSelected?.Invoke(this, emoji);
            }
        }
    }
}
