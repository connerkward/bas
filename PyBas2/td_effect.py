# TouchDesigner Effect Script
# Connect this file to a Text DAT with Sync to File = Read
# Then right-click the Text DAT → Run Script

# ============================================
# UNCOMMENT AND MODIFY THE CODE BELOW TO TEST
# ============================================

print("S")

# Example: Create a noise texture
parent = op('/project1')
noise = parent.create(noiseTOP, 'noise1')
noise.par.monochrome = True

# Example: Set parameter on existing node
op('/project1/noise1').par.roughness = 0.5

# Example: Connect two nodes
op('/project1/noise1').outputConnectors[0].connect(op('/project1/blur1'))

