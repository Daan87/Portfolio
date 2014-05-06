void Game::UpdateRadar()
{
	for (int i = 0; i < 2; i++)
	{
		Horde3D::Vec3f disVec = Horde3D::Vec3f(m_CamX, m_CamY, m_CamZ) - m_radarPos[i];
		disVec.y = 0;
		float distance = disVec.length();

		if(distance < 130)
		{
			float angleToObject = atan2( m_radarPos[i].z - m_CamZ, m_radarPos[i].x - m_CamX );
			angleToObject += degToRad( m_CamRotationY );

			float radarX = 1160 + (cos( angleToObject )/2) * distance;
			float radarZ = 625 + (sin( angleToObject )/2) * distance;

			float xPos = radarX;
			float zPos = radarZ;

			float Corners[16];
			CalculateCorners( *m_Cam, Corners, xPos - 5, zPos - 5, 10, 10 );

			//const float ww = (float)h3dGetNodeParamI( *m_Cam, H3DCamera::ViewportWidthI ) / (float)h3dGetNodeParamI( *m_Cam, H3DCamera::ViewportHeightI );
			//const float radarCorners[] = { ww-0.16f, 0.86f, 0, 1, ww-0.18f, 0.87, 0, 0, ww-0.15f, 0.87, 1, 0, ww-0.15f, 0.86f, 1, 1 };

			h3dShowOverlays( Corners, 4, 1.f, 1.f, 1.f, 1.f, AssetManager::SharedManager()->UIElements[1], 0 );
		}
	}
}

void Game::CalculateCorners( H3DNode a_cam,  float a_Array[16], float a_X, float a_Y, float a_Width, float a_Height )
{
       float ww = (float)h3dGetNodeParamI( a_cam, H3DCamera::ViewportWidthI ) / (float)h3dGetNodeParamI( a_cam, H3DCamera::ViewportHeightI );
       int camerawidth = h3dGetNodeParamI( a_cam, H3DCamera::ViewportWidthI );
       int cameraheight = h3dGetNodeParamI( a_cam, H3DCamera::ViewportHeightI );
       camerawidth = (int)(camerawidth / ww);

       a_Array[0] = a_X / camerawidth;
       a_Array[1] = a_Y / cameraheight;
       a_Array[2] = 0;
       a_Array[3] = 1;
       a_Array[4] = a_X / camerawidth;
       a_Array[5] = (a_Y + a_Height) / cameraheight;
       a_Array[6] = 0;
       a_Array[7] = 0;
       a_Array[8] = (a_X + a_Width) / camerawidth;
       a_Array[9] = (a_Y + a_Height) / cameraheight;
       a_Array[10] = 1;
       a_Array[11] = 0;
       a_Array[12] = (a_X + a_Width) / camerawidth;
       a_Array[13] = a_Y / cameraheight;
       a_Array[14] = 1;
       a_Array[15] = 1;
}