"""
Random Strava Comments Generator for TouchDesigner

SETUP:
1. Create a Text DAT named 'strava_comments'
2. Create another Text DAT with this script
3. Run: op('this_script').run()

Or set up a Timer CHOP to auto-generate new comments
"""

import random

# Strava comment templates
STRAVA_COMMENTS = [
    "💪💪💪",
    "Nice work!",
    "Crushing it! 🔥",
    "Beast mode!",
    "Strong! 💪",
    "Keep it up!",
    "Solid effort!",
    "Looking strong out there",
    "Great pace!",
    "Love the consistency",
    "🔥🔥🔥",
    "Killed it!",
    "That's what I'm talking about!",
    "PR szn",
    "Legs of steel",
    "Making it look easy",
    "Respect the grind",
    "You're an animal!",
    "Absolutely sending it",
    "Sub 7 min mile? Wild",
    "That elevation gain 📈",
    "Absolute weapon",
    "Built different",
    "How are you so fast??",
    "Save some for the rest of us",
    "Wow just wow",
    "Meanwhile I'm on the couch",
    "Jealous of those splits",
    "Goals right here",
    "This is insane",
    "Can't believe this pace",
    "🐐",
    "Legend",
    "Machine",
    "Superhuman",
    "How",
    "No way",
    "????",
    "Stop it",
    "Okay calm down",
    "Easy day?? 😂",
    "\"Recovery run\" sure",
    "My PR is your warmup",
    "RIP your legs tomorrow",
    "Ice bath szn",
    "Nutrition on point",
    "Form is looking clean",
    "Cadence 👌",
    "That negative split though",
    "PR or ER vibes",
    "Different breed",
    "Built by science",
    "Engineered for speed",
]

# Strava usernames
USERNAMES = [
    "Tyler_Runs",
    "CoachMike",
    "RunnerGirl_89",
    "SpeedDemon",
    "TrailBlazer_23",
    "FitnessFreak",
    "MarathonMike",
    "5amClub",
    "NeverSkipLegDay",
    "StravaAddict",
    "PRChaser",
    "EnduranceAthlete",
    "PaceSetter",
    "MilesMatter",
    "TheGrindNeverStops",
]


def generate_comment():
    """Generate a single random Strava comment with username"""
    username = random.choice(USERNAMES)
    comment = random.choice(STRAVA_COMMENTS)
    return f"{username}: {comment}"


def generate_comments(count=10):
    """Generate multiple random Strava comments"""
    comments = []
    for _ in range(count):
        comments.append(generate_comment())
    return "\n\n".join(comments)


def write_to_dat(output_dat_path='strava_comments', count=10):
    """
    Generate comments and write to Text DAT
    
    Args:
        output_dat_path: Name/path of Text DAT to write to
        count: Number of comments to generate
    """
    comments = generate_comments(count)
    
    output_dat = op(output_dat_path)
    if output_dat is None:
        print(f"ERROR: Text DAT '{output_dat_path}' not found")
        return
    
    output_dat.text = comments
    print(f"Generated {count} Strava comments")


# ========== USAGE ==========

def run(count=10):
    """
    Default run function
    Call from button, timer, or script: op('this_script').run()
    """
    write_to_dat('strava_comments', count=count)


def run_continuous():
    """
    Add a new comment to existing text (for continuous updates)
    """
    output_dat = op('strava_comments')
    if output_dat is None:
        print("ERROR: Text DAT 'strava_comments' not found")
        return
    
    new_comment = generate_comment()
    
    # Prepend new comment (newest at top)
    if output_dat.text:
        output_dat.text = new_comment + "\n\n" + output_dat.text
    else:
        output_dat.text = new_comment
    
    # Optional: Limit total comments to prevent infinite growth
    lines = output_dat.text.split("\n\n")
    if len(lines) > 20:
        output_dat.text = "\n\n".join(lines[:20])


# Manual test (uncomment to run immediately when script loads)
# run(15)
