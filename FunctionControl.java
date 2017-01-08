package daan.games.heat_mapping;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.geom.Line2D;
import java.io.*;
import javax.swing.*;
import javax.xml.parsers.*;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import org.w3c.dom.*;
import org.eclipse.swt.*;
import org.eclipse.swt.events.*;
import org.eclipse.swt.graphics.*;
import org.eclipse.swt.widgets.*;

public class FunctionControl
{
	//Directory info
	final JFileChooser m_fc = new JFileChooser();
	private int m_state = JFileChooser.ERROR_OPTION;
	private File[] m_dirList;
	
	//Draw line
	double[] m_line;
	
	//Settings
	boolean m_speed;
	boolean m_transparency;
	boolean m_hotspots;
	boolean m_showAll;
	boolean m_playPath;
	
	//Transparent path
	Path path1;
	
	//Play path
	int m_play;
	
	//Constructor
    public FunctionControl() 
    {
    	//Settings file chooser
    	//setFileChooser();
    	
    	//Settings
    	m_speed = false;
    	m_transparency = false;
    	m_hotspots = false;
    	m_showAll = false;
    	m_playPath = false;
    	
    	//transparent path
    	path1 = null;
    }
    
    /**
     * Main functions for buttons
     */
    
    //Let the user choose a folder in which the xml files with data are placed
    protected void fillList(org.eclipse.swt.widgets.List playerList)
	{
    	playerList.removeAll();
    	playerList.redraw();
    	
    	m_dirList = chooseFolder();
    	
		if (m_dirList != null) 
		{
		    for (File child : m_dirList) 
		    {
		    	playerList.add(child.getName().replace(".xml", ""));
		    }
		} 
		else 
		{
		}
	}
    
    //Load the data from the xml file
    public double[] getData(int i, Rectangle c)
    {
    	try
    	{
    		//Load the data from the file
    		int gpsDataCounter = 0;
    		
	    	File gpsData = new File(m_dirList[i].getAbsolutePath());
	    	DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
	    	DocumentBuilder dBuilder = dbFactory.newDocumentBuilder();
	    	Document doc = dBuilder.parse(gpsData);
	    	
	    	doc.getDocumentElement().normalize();
	     
	    	NodeList nList = doc.getElementsByTagName("Location");
	    	
	    	int listSize = 0;
	    	
	    	//Loop through all the data in the file
	    	for (int temp = 0; temp < nList.getLength(); temp++) 
	    	{
	    		Node nNode = nList.item(temp);
	    		
	    		if (nNode.getNodeType() == Node.ELEMENT_NODE) 
	    		{
	    			Element eElement = (Element) nNode;
	    			//Check the acc
	    			if(Double.parseDouble(eElement.getAttribute("Acc")) <= 16.0)
	    			{
	    				listSize++;
	    			}
	    		}
	    		
	    		m_line = new double[listSize*3];
	    	}
	    	
	    	//Loop through the data and place it in the new array
	    	for (int temp = 0; temp < nList.getLength(); temp++) 
	    	{
	    		Node nNode = nList.item(temp);
	     
	    		if (nNode.getNodeType() == Node.ELEMENT_NODE) 
	    		{
	    			Element eElement = (Element) nNode;
	    			
	    			if(Double.parseDouble(eElement.getAttribute("Acc")) <= 16.0)
	    			{
	    				//Calculate the X
		    			double difX = 4.8606428 - Double.parseDouble(eElement.getAttribute("Longitude"));
		    			double finX = difX / 0.0018293;
		    			int x = c.width - (int)(c.width * finX);
		    			if(x >= 0 && x < c.width)
		    			{
		    				m_line[gpsDataCounter] = x;
		    			}
		    			else if(x >= c.width)
		    			{
		    				m_line[gpsDataCounter] = c.width + 5;
		    			}
		    			else
		    			{
		    				m_line[gpsDataCounter] = 5;
		    			}
		    			
		    			//Calculate the Y
		    			double difY = 51.6145973 - Double.parseDouble(eElement.getAttribute("Latitude"));
		    			double finY = difY / 0.0010509;
				        int y = (int)(c.height * finY );
				        if(y >= 0 && y < c.height)
		    			{
				        	m_line[gpsDataCounter+1] = y;
		    			}
		    			else if(y >= c.height)
		    			{
		    				m_line[gpsDataCounter+1] = c.height - 5;
		    			}
		    			else
		    			{
		    				m_line[gpsDataCounter+1] = 5;
		    			}
				        
				        //Get the speed
				        double speed = Double.parseDouble(eElement.getAttribute("Speed"));;
				        m_line[gpsDataCounter+2] = speed;
				        
				        gpsDataCounter = gpsDataCounter + 3;
	    			}
	    		}
	    	}
    	}
    	catch(Exception e)
    	{
    		System.out.println(e.getMessage());
    	}
    	
    	return m_line;
    }
	
    //Draw Path
  	public void drawPath(int dataIndex, Rectangle clientArea, PaintEvent e, Display display, Path path)
  	{  		
  		double[] line = getData(dataIndex, clientArea);
      	
  		int colorIndex = getColor(dataIndex);
  		
  		e.gc.setBackground(e.display.getSystemColor(colorIndex));
      	e.gc.setForeground(e.display.getSystemColor(colorIndex));

      	if(m_transparency)
      	{
      		path1 = new Path(display);
      	}

      	//Attributes for circles when player is standing still
      	int lineWidth = 2;
      	int circleCount = (int)(lineWidth * 4);
      	
      	if(line != null)
      	{  		
      		for (int i = 0; i < line.length; i = i+3) 
  	    	{
      			if(i+5 < line.length)
      			{
      				//Draw the path the player walked
  					path = new Path(display);
          			path.moveTo((int)line[i], (int)line[i+1]);
          			path.lineTo((int)line[i+3], (int)line[i+4]);
          			path.close();
          			
          			//Set the attributes of the path
          			e.gc.setAntialias(SWT.ON);
      			    e.gc.setLineJoin(SWT.JOIN_ROUND);
      			    
      			    //Set the line width to the speed if selected
      			    if(m_speed)
    				{
      			    	int lineSpeed = lineWidth;
      			    	
      			    	if(line[i+2] != 0)
      			    	{
      			    		lineSpeed = (int)(lineWidth / line[i+2]);
      			    	}
      			    	
      			    	e.gc.setLineWidth(lineSpeed);
    				}
      			    else
      			    {
      			    	e.gc.setLineWidth(lineWidth);
      			    }
      			    
      			    //When the path is transparent draw the intersecting points
      			    if(m_transparency)
      			    {
      			    	//Calculate starting point
      					float vx = (int)line[i+3] - (int)line[i];
      					float vy = (int)line[i+4] - (int)line[i+1];
      					float next_point_x = (float)(line[i] + vx*0.1);
      					float next_point_y = (float)(line[i+1] + vy*0.1);
      					
      					//Get the intersecting point
      					Point intersectPoint = getIntersection(line, (float)next_point_x, (float)next_point_y, (int)line[i+3], (int)line[i+4]);
      					
      					if(intersectPoint != null)
      					{
      						//Set the alpha and draw the circle
      						e.gc.setAlpha(155);
      						e.gc.drawOval((int)(intersectPoint.x - (lineWidth * 0.5)), (int)(intersectPoint.y - (lineWidth * 0.5)), lineWidth, lineWidth);
      					}
      					
      					e.gc.setAlpha(255);
      					
      					path1.addPath(path);
      			    }      				
      			    else
      			    {
      			    	//Draw the path
      			    	e.gc.drawPath(path);  
      			    }
      			    
      				
      			          				
      				//Draw circles when the player is standing still somewhere
      				if(m_hotspots)
      				{
      					if(m_transparency)
      					{
      						e.gc.setAlpha(155);
      					}
	      				if(line[i] == line[i+3] && line[i+1] == line[i+4])
	      				{
	      					circleCount++;
	      					e.gc.fillOval((int)line[i]-(int) (circleCount * 0.5),(int)line[i+1]-(int) (circleCount * 0.5),circleCount,circleCount);
	      				}
	      				else
	      				{
	      					circleCount = (int)(lineWidth * 4);
	      				}
      				}
      			}
  	    	}
      		
      		if(m_transparency)
      		{
	      		path1.close();
	  			e.gc.setAlpha(155);
				e.gc.drawPath(path1);
      		}
				
      	}
  	}
  	
    //Save the map as it is shown on the screen
	public void saveMap(Canvas canvas, Display display)
	{
		//Set Filechooser
		setFileChooser(JFileChooser.FILES_ONLY, JFileChooser.SAVE_DIALOG, getSettings(1));
		
		//Create a dialog window with the filechooser and place it on top of everything.
		JDialog dialog = new JDialog();
        dialog.setAlwaysOnTop(true);
        dialog.setTitle("Save Map");
        dialog.setModal(true);
        dialog.add(m_fc);
        dialog.pack();
        dialog.setLocationRelativeTo(null);
        dialog.setVisible(true);
        
        File dir = null;
        
        switch (m_state) 
        {
	        case JFileChooser.APPROVE_OPTION:
	        	dir = m_fc.getSelectedFile();
	        	saveSettings(1, dir.getAbsolutePath());
	        	//System.out.println(dir.getAbsolutePath());
	        	break;
	        case JFileChooser.CANCEL_OPTION:
	            System.out.println("cancelled");
	            break;
	        default:
	            System.out.println("Broken");
	            break;
        }
        
        
        //Create an image from the canvas
		GC gc = new GC(canvas);
        Image image = new Image(display, canvas.getSize().x,canvas.getSize().y);
        gc.copyArea(image, 0, 0);
        ImageLoader loader = new ImageLoader();
        loader.data = new ImageData[] { image.getImageData() };
        if(dir != null)
        {
        	loader.save(dir.getAbsolutePath() + ".jpg", SWT.IMAGE_JPEG);
        }
        else
        {
        	loader.save("Map.jpg", SWT.IMAGE_JPEG);
        }
        gc.dispose();
	}
	
	//Shutdown the program
	public void closeProgram()
	{
		System.exit(0);
	}
	
	/**
	 * supporting functions
	 */
	
	//Choose the folder in which the xmls with gps data are
	public File[] chooseFolder()
    {
		setFileChooser(JFileChooser.DIRECTORIES_ONLY, JFileChooser.OPEN_DIALOG, getSettings(2));
		
    	//Create a dialog window with the filechooser and place it on top of everything.
    	JDialog dialog = new JDialog();
        dialog.setAlwaysOnTop(true);
        dialog.setTitle("Choose Folder");
        dialog.setModal(true);
        dialog.add(m_fc);
        dialog.pack();
        dialog.setLocationRelativeTo(null);
        dialog.setVisible(true);
        
        switch (m_state) 
        {
	        case JFileChooser.APPROVE_OPTION:
	        	File dir = m_fc.getSelectedFile();
	        	File[] dirList = dir.listFiles();
	        	saveSettings(2, dir.getAbsolutePath());
	        	//System.out.println(dir.getAbsolutePath());
	        	return dirList;
	        case JFileChooser.CANCEL_OPTION:
	            System.out.println("cancelled");
	            return null;
	        default:
	            System.out.println("Broken");
	            return null;
        }
    }
	
	//Settings FileChooser
	public void setFileChooser(int selectMode, int dialogMode, String path)
	{
		//Set the FileChooser to directories selection
    	m_fc.setFileSelectionMode(selectMode);
    	m_fc.setDialogType(dialogMode);
    	//m_fc.setSelectedFile(new File(path));
    	
    	//Add an actionlistener to the file chooser, with this we can see what action the user takes and close the window if necessary
    	m_fc.addActionListener(new ActionListener() 
    	{
            public void actionPerformed(ActionEvent e) 
            {
                if (JFileChooser.CANCEL_SELECTION.equals(e.getActionCommand())) 
                {
                	m_state = JFileChooser.CANCEL_OPTION;
                    SwingUtilities.windowForComponent((JFileChooser) e.getSource()).dispose();
                } 
                else if (JFileChooser.APPROVE_SELECTION.equals(e.getActionCommand())) 
                {
                	m_state = JFileChooser.APPROVE_OPTION;
                    SwingUtilities.windowForComponent((JFileChooser) e.getSource()).dispose();
                }
            }
        });
	}
	
	//Choose a color
	private int getColor(int colorIndex)
	{
		switch (colorIndex) 
		{
        	case 1:
        		return SWT.COLOR_BLACK;
        	case 2:
        		return SWT.COLOR_BLUE;
        	case 3:
        		return SWT.COLOR_CYAN;
        	case 4:
        		return SWT.COLOR_DARK_BLUE;
        	case 5:
        		return SWT.COLOR_DARK_CYAN;
        	case 6:
        		return SWT.COLOR_DARK_GRAY;
        	case 7:
        		return SWT.COLOR_DARK_GREEN;
        	case 8:
        		return SWT.COLOR_DARK_MAGENTA;
        	case 9:
        		return SWT.COLOR_DARK_RED;
        	case 10:
        		return SWT.COLOR_DARK_YELLOW;
        	case 11:
        		return SWT.COLOR_GRAY;
        	case 12:
        		return SWT.COLOR_GREEN;
        	case 13:
        		return SWT.COLOR_MAGENTA;
        	case 14:
        		return SWT.COLOR_RED;
        	case 15:
        		return SWT.COLOR_WHITE;
        	case 0:
        		return SWT.COLOR_YELLOW;
		}
		
		return SWT.COLOR_BLACK;
		
	}

	//Save last chosen folder 
	public void saveSettings(int settingsSort, String path)
	{
		try
		{
			//Create the settings xml
			File xmlFile = new File("Settings/Settings.xml");  
			DocumentBuilderFactory documentFactory = DocumentBuilderFactory.newInstance();  
			DocumentBuilder documentBuilder = documentFactory.newDocumentBuilder();  
			Document doc = documentBuilder.parse(xmlFile);
			
			Node data = null;
			
			//Place the data
			switch(settingsSort)
			{
				case 1:
					//map
					data = doc.getElementsByTagName("saveMap").item(0);
					break;
				case 2:
					//folder
					data = doc.getElementsByTagName("openFiles").item(0);
					break;
			}
	 
			//Add the data to the right attribute
			NamedNodeMap attr = data.getAttributes();
			Node nodeAttr = attr.getNamedItem("path");
			nodeAttr.setTextContent(path);
			
			//Place the data in an xml file
			TransformerFactory transformerFactory = TransformerFactory.newInstance();
			Transformer transformer = transformerFactory.newTransformer();
			DOMSource source = new DOMSource(doc);
			StreamResult result = new StreamResult(new File(xmlFile.getAbsolutePath()));
			transformer.transform(source, result);
		}
		catch(Exception e)
		{
			System.out.println(e.getMessage());
		}
	}
	
	//Get last chosen folder
	public String getSettings(int settingsSort)
	{
		//Get the settings xml file and read the data
		try
		{
			File xmlFile = new File("Settings/Settings.xml");  
			DocumentBuilderFactory documentFactory = DocumentBuilderFactory.newInstance();  
			DocumentBuilder documentBuilder = documentFactory.newDocumentBuilder();  
			Document doc = documentBuilder.parse(xmlFile);
			
			Node data = null;
			
			switch(settingsSort)
			{
				case 1:
					//map
					data = doc.getElementsByTagName("saveMap").item(0);
					break;
				case 2:
					//folder
					data = doc.getElementsByTagName("openFiles").item(0);
					break;
			}
	 
			//Get the correct attribute
			NamedNodeMap attr = data.getAttributes();
			Node nodeAttr = attr.getNamedItem("path");
			return nodeAttr.getTextContent();
		}
		catch(Exception e)
		{
			System.out.println(e.getMessage());
		}
		
		return "";
	}

	//Calculate the path intersections
	private Point getIntersection(double[] line, float x1, float y1, int x2, int y2)
	{
		//Create the first line
		Line2D.Double line1 = new Line2D.Double(x1, y1, x2, y2);
		
		//Loop through all the lines
		for (int i = 0; i < line.length; i = i+3) 
	    {
  			if(i+3 < line.length)
  			{
  				//Check if the line is not intersecting with itself
  				if(x1 != x2 && y1 != y2)
  				{
  					if(x1 != line[i] && y1 != line[i+1] && x2 != line[i+3] && y2 != line[i+4])
  					{
  						if(x2 != line[i] && y2 != line[i+1])
  						{
  							//When the line is not checking itself check if there is an intersection with another line
  							Line2D.Double line2 = new Line2D.Double(line[i], line[i+1], line[i+3], line[i+4]);
  							Point result = getLineIntersection(line1, line2);
  							
  							//When there is an intersection return result
  							if(result != null)
  							{
  								//System.out.print(result);
  								return result;
  							}
  						}
  					}
  				}
  				else
  				{
  					//return false;
  				}
  			}
	    }
		
		return null;
	}
	
	static Point getLineIntersection(Line2D.Double pLine1, Line2D.Double pLine2)
	{
	    Point
	        result = null;

	    double
	        s1_x = pLine1.x2 - pLine1.x1,
	        s1_y = pLine1.y2 - pLine1.y1,

	        s2_x = pLine2.x2 - pLine2.x1,
	        s2_y = pLine2.y2 - pLine2.y1,

	        s = (-s1_y * (pLine1.x1 - pLine2.x1) + s1_x * (pLine1.y1 - pLine2.y1)) / (-s2_x * s1_y + s1_x * s2_y),
	        t = ( s2_x * (pLine1.y1 - pLine2.y1) - s2_y * (pLine1.x1 - pLine2.x1)) / (-s2_x * s1_y + s1_x * s2_y);

	    if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
	    {
	        result = new Point(
	            (int) (pLine1.x1 + (t * s1_x)),
	            (int) (pLine1.y1 + (t * s1_y)));
	    }

	    return result;
	}
}
