import GUI
import BigWorld
import Keys
import chapters
import random
import Math
import Pixie

from Helpers import ChatConsole

class Avatar( BigWorld.Entity ):

	health = 100
	shaderValue = 1.0
	spawnLocations = ((63.687,3.069,-146.898), (73.189,1.089,1.533), (-0.980,4.497,-159.378), (-44.457,1.843,-70.632))

	def onEnterWorld( self, prereqs ):
		# Set the position/movement filter to correspond to an avatar
		self.filter = BigWorld.AvatarFilter()

		# Load up the bipedgirl model
		self.model = BigWorld.Model( "characters/mortar/Anne_Mortar.model" )

		# GUI
		self.openHealthBar()

	def say( self, msg ):

		ChatConsole.ChatConsole.instance().write(
			"%d says: %s" % (self.id, msg) )

	# Let the avatar take damage
	def takeDamage( self , damage ):
		self.health = self.health - 10
		self.healthBarTakeDamage(0.1)
		if self.health < 0:
			self.respawn()
	
	# Respawn at a dandom location
	def respawn( self ):
		self.shaderValue = 1.0
		self.health = 100
		self.randomIndex = random.randint(0,3)
		self.physics.teleport(self.spawnLocations[self.randomIndex])

	################################################################################################## 
	##Code Dirk Heijn 																				##
	################################################################################################## 
	
	#open the GUI file and add the healthbar onscreen
	#also adds clip and colour shaders to animate the size and hue of the healthbar when taking damage
	def openHealthBar( self ):

		self.healthBar = GUI.load( "guis/healthBar.gui" )
		
		self.hbClipper = GUI.ClipShader("RIGHT")
		self.hbClipper.speed = 1
		self.hbClipper.value = 1
		
		self.hbColour = GUI.ColourShader()
		self.hbColour.start =(255,0,0,220)
		self.hbColour.middle = (120,120,0,220)
		self.hbColour.end = (0,255,0,220)
		self.hbColour.speed = 1
		self.hbColour.value = 1

		self.healthBar.addShader(self.hbClipper)
		self.healthBar.addShader(self.hbColour)
		GUI.addRoot(self.healthBar)

	#amount = the amount of damage in a hit
	def healthBarTakeDamage( self, amount ):
		self.shaderValue = self.shaderValue - amount
		self.hbColour.value = self.shaderValue
		self.hbClipper.value = self.shaderValue
		print "current shader value = "+str(self.shaderValue)
		
	################################################################################################## 
	##################################################################################################

class PlayerAvatar( Avatar ):
	
	base_speed = 8.0
	excelleration = 1.0
	movement_speed = base_speed
	isDown = False
	
	def onEnterWorld( self, prereqs ):
		
		Avatar.onEnterWorld( self, prereqs )

		# Set the position/movement filter to correspond to an player avatar
		self.filter = BigWorld.PlayerAvatarFilter()

		# Setup the physics for the Avatar
		self.physics = BigWorld.STANDARD_PHYSICS
		self.physics.velocityMouse = "Direction"
		self.physics.oldStyleCollision = True
		self.physics.collide = True
		self.physics.collideTerrain = True
		self.physics.collideObjects = True
		self.physics.fall = True

		# Spawn
		self.spawnAtRandomLocation()

		# Dust particles
		self.dustTrail = Pixie.create ("particles/dust_trail.xml")
		self.attachDustTrail()

	def handleKeyEvent( self, event ):

		self.isDown = event.isKeyDown()

		# reset the information when the button is released
		if event.key == Keys.KEY_W and self.isDown:
			#do nothing
			print "nothing"
		else:
			self.excelleration = 1.0
			self.movement_speed = self.base_speed

		# Get the current velocity
		v = self.physics.velocity

		# Update the velocity depending on the key input
		if event.key == Keys.KEY_W:
			self.updateExcelleration()
			v.z = self.isDown * self.movement_speed

		# Save back the new velocity
		self.physics.velocity = v

		# Shoot
		if event.key == Keys.KEY_LEFTMOUSE:

			if event.isRepeatedEvent():
				return

			down = event.isKeyDown()
		
			if down:
				self.model.Shoot()
				# create the bullet entity
				self.throwDynamicObject(self.model.position)
				self.getMouseTargettingRay()
	
	# update the speed of the vehicle
	def updateExcelleration( self ):
		if self.excelleration < 4.0:
			self.excelleration = self.excelleration + 0.01
			self.movement_speed = self.base_speed * self.excelleration

	# Create a new entity server sided
	def throwDynamicObject( self , position ):
		direction = (self.roll,  self.pitch, self.yaw )
		self.base.spawnEntity(position, direction)

	# Set the destination for the bullet
	def getMouseTargettingRay( self ):
		mtm = Math.Matrix( BigWorld.MouseTargettingMatrix() )
		src = mtm.applyToOrigin()
		far = BigWorld.projection().farPlane
		dst = src + mtm.applyToAxis(2).scale( far )
		self.base.setBulletDST(dst)
	
	# Spawning locations
	def spawnAtRandomLocation(self):
		self.randomIndex = random.randint(0,3)
		self.physics.teleport(self.spawnLocations[self.randomIndex])

# Avatar.py
