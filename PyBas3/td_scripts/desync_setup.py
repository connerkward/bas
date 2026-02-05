"""
Desync: Chronophoto with sync/desync cycle.
Run in TD Textport: exec(open('/Users/CONWARD/dev/bas/PyBas3/td_scripts/desync_setup.py').read())
"""
import td

VIDEO_PATH = '/Users/CONWARD/dev/bas/input_videos/runside-megaslow-compressed.mp4'
VIDEO_LENGTH = 4585

def setup():
    p1 = op('/project1')
    if not p1:
        p1 = parent().create(containerCOMP, 'project1')

    c = p1.op('desync')
    if not c:
        c = p1.create(containerCOMP, 'desync')

    # --- CONTROLS ---
    sync_hold = c.op('sync_hold') or c.create(constantCHOP, 'sync_hold')
    sync_hold.par.length = 1
    sync_hold.par.chans = 1
    sync_hold.par.vals = [3]
    sync_hold.par.name1 = 'sec'

    desync_dur = c.op('desync_duration') or c.create(constantCHOP, 'desync_duration')
    desync_dur.par.length = 1
    desync_dur.par.chans = 1
    desync_dur.par.vals = [8]
    desync_dur.par.name1 = 'sec'

    spread = c.op('spread') or c.create(constantCHOP, 'spread')
    spread.par.length = 1
    spread.par.chans = 1
    spread.par.vals = [500]
    spread.par.name1 = 'frames'

    baseline_fps = c.op('baseline_fps') or c.create(constantCHOP, 'baseline_fps')
    baseline_fps.par.length = 1
    baseline_fps.par.chans = 1
    baseline_fps.par.vals = [30]
    baseline_fps.par.name1 = 'fps'

    # --- DESYNC MOD SCRIPT ---
    cb = c.op('desync_mod_callbacks')
    if not cb:
        cb = c.create(textDAT, 'desync_mod_callbacks')
    cb.text = '''
def onCook(scriptOp):
    scriptOp.clear()
    t = absTime.seconds
    sh = op('/project1/desync/sync_hold')
    hold = sh['sec'][0] if sh else 3
    dd = op('/project1/desync/desync_duration')
    desync_len = dd['sec'][0] if dd else 8
    cycle = hold * 2 + desync_len
    pos = t % cycle
    if pos < hold or pos > (hold + desync_len):
        desync = 0
    else:
        x = (pos - hold) / desync_len
        linear = 1 - abs(2 * x - 1)
        desync = linear * linear * (3 - 2 * linear)
    ch = scriptOp.appendChan('desync')
    ch[0] = desync
'''

    dm = c.op('desync_mod')
    if not dm:
        dm = c.create(scriptCHOP, 'desync_mod')
    dm.par.script = "op('desync_mod_callbacks')"
    dm.par.callbacks = 1

    # --- TIMELINES ---
    mults = [-1, -0.5, 0, 0.5, 1]
    for i in range(5):
        t = c.op('timeline' + str(i))
        if not t:
            t = c.create(moviefileinTOP, 'timeline' + str(i))
        t.par.file = VIDEO_PATH
        expr = "int(absTime.seconds * op('/project1/desync/baseline_fps')['fps'] + {} * op('/project1/desync/spread')['frames'] * op('/project1/desync/desync_mod')['desync']) % {}".format(mults[i], VIDEO_LENGTH)
        t.par.index.expr = expr

    # --- MONO (per DESYNC_V2: grayscale for clean min blend) ---
    for i in range(5):
        mono = c.op('mono' + str(i))
        if not mono:
            mono = c.create(monochromeTOP, 'mono' + str(i))
        if not mono.inputConnectors[0].inputs:
            c.op('timeline' + str(i)).outputConnectors[0].connect(mono)

    # --- BACKGROUND ---
    bg = c.op('background')
    if not bg:
        bg = c.create(constantTOP, 'background')
    bg.par.resolutionw = 720
    bg.par.resolutionh = 1280
    bg.par.colorr = 1
    bg.par.colorg = 1
    bg.par.colorb = 1
    bg.par.colora = 1

    # --- COMPOSITE OUT ---
    out_comp = c.op('out')
    if not out_comp:
        out_comp = c.create(compositeTOP, 'out')
    out_comp.par.operand = 'minimum'
    out_comp.par.top = c.op('mono0')
    out_comp.par.top2 = c.op('mono1')
    out_comp.par.top3 = c.op('mono2')
    out_comp.par.top4 = c.op('mono3')
    out_comp.par.top5 = c.op('mono4')
    out_comp.par.top6 = bg

    # --- RED TINT ---
    red = c.op('red_tint_level')
    if not red:
        red = c.create(levelTOP, 'red_tint_level')
    red.par.gainr = 1
    red.par.gaing = 0
    red.par.gainb = 0
    if not red.inputConnectors[0].inputs:
        out_comp.outputConnectors[0].connect(red.inputConnectors[0])

    # --- POST MONO ---
    post_m = c.op('post_mono')
    if not post_m:
        post_m = c.create(monochromeTOP, 'post_mono')
    if not post_m.inputConnectors[0].inputs:
        red.outputConnectors[0].connect(post_m.inputConnectors[0])

    # --- OUTPUT NULLS ---
    out_col = p1.op('out_color')
    if not out_col:
        out_col = p1.create(nullTOP, 'out_color')
    if not out_col.inputConnectors[0].inputs:
        red.outputConnectors[0].connect(out_col.inputConnectors[0])

    out_mo = p1.op('out_mono')
    if not out_mo:
        out_mo = p1.create(nullTOP, 'out_mono')
    if not out_mo.inputConnectors[0].inputs:
        post_m.outputConnectors[0].connect(out_mo.inputConnectors[0])

    print('Desync setup complete: out_color (red), out_mono (red+bw)')

setup()
