"""
Display ONE random Strava comment on screen
Updates every time you run it
"""

import random

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

def run():
    """Pick one random comment and write to Text DAT"""
    comment = random.choice(STRAVA_COMMENTS)
    
    output_dat = op('comment_output')
    if output_dat is None:
        print("ERROR: Text DAT 'comment_output' not found")
        return
    
    output_dat.text = comment
