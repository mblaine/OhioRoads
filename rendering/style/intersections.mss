@endcap-scale-lte12: 0.087;
@endcap-scale-lte13: 0.18;
@endcap-scale-lte14: 0.28;
@endcap-scale-15: 0.384;
@endcap-scale-16: 0.722;
@endcap-scale-gte17: 0.9;

@endcap-width: 64;
@endcap-height: 17;

@separator-width: 10;
@separator-height: 64;


@endcap-offset-large-15: '-10'; /*-10*/
@endcap-offset-small-15: '-4'; /*-4*/

@endcap-offset-large-gte16: '-20'; /*-20*/
@endcap-offset-small-gte16: '-8'; /*-8*/

#intersections[connections=null]
{
	marker-file: url("sprites/cap.png");
	marker-allow-overlap: true;
	marker-placement: point;
	
	[zoom<=15]
	{
		marker-transform: 'rotate([nameEndAngle0]) translate(0 ' + @endcap-offset-large-15 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle0]) translate(0 ' + @endcap-offset-small-15 + ')';}
	}
	
	[zoom>=16]
	{
		marker-transform: 'rotate([nameEndAngle0]) translate(0 ' + @endcap-offset-large-gte16 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle0]) translate(0 ' + @endcap-offset-small-gte16 + ')';}
	}
	
	[zoom<=14] 
	{ 
		marker-width: @endcap-scale-lte14 * @endcap-width;
		marker-height: @endcap-scale-lte14 * @endcap-height;
	}
	[zoom=15] 
	{ 
		marker-width: @endcap-scale-15 * @endcap-width;
		marker-height: @endcap-scale-15 * @endcap-height;
	}
	[zoom=16] 
	{ 
		marker-width: @endcap-scale-16 * @endcap-width;
		marker-height: @endcap-scale-16 * @endcap-height;
	}
	[zoom>=17] 
	{ 
		marker-width: @endcap-scale-gte17 * @endcap-width;
		marker-height: @endcap-scale-gte17 * @endcap-height;
	}
}

#intersections[connections=2]
{
	marker-file: url("sprites/separator.png");
	marker-allow-overlap: true;
	marker-placement: point;
	marker-transform: 'rotate([separatorAngle])';

	[zoom<=14] 
	{ 
		marker-width: @endcap-scale-lte14 * @separator-width;
		marker-height: @endcap-scale-lte14 * @separator-height;
	}
	[zoom=15] 
	{ 
		marker-width: @endcap-scale-15 * @separator-width;
		marker-height: @endcap-scale-15 * @separator-height;
	}
	[zoom=16] 
	{ 
		marker-width: @endcap-scale-16 * @separator-width;
		marker-height: @endcap-scale-16 * @separator-height;
	}
	[zoom>=17] 
	{ 
		marker-width: @endcap-scale-gte17 * @separator-width;
		marker-height: @endcap-scale-gte17 * @separator-height;
	}
}

#intersections[nameEndAngle1!=null][connections=null]::cap1
{
	marker-file: url("sprites/cap.png");
	marker-allow-overlap: true;
	marker-placement: point; 

	[zoom<=15]
	{
		marker-transform: 'rotate([nameEndAngle1]) translate(0 ' + @endcap-offset-large-15 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle1]) translate(0 ' + @endcap-offset-small-15 + ')';}
	}
	
	[zoom>=16]
	{
		marker-transform: 'rotate([nameEndAngle1]) translate(0 ' + @endcap-offset-large-gte16 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle1]) translate(0 ' + @endcap-offset-small-gte16 + ')';}
	}

	[zoom<=14] 
	{ 
		marker-width: @endcap-scale-lte14 * @endcap-width;
		marker-height: @endcap-scale-lte14 * @endcap-height;
	}
	[zoom=15] 
	{ 
		marker-width: @endcap-scale-15 * @endcap-width;
		marker-height: @endcap-scale-15 * @endcap-height;
	}
	[zoom=16] 
	{ 
		marker-width: @endcap-scale-16 * @endcap-width;
		marker-height: @endcap-scale-16 * @endcap-height;
	}
	[zoom>=17] 
	{ 
		marker-width: @endcap-scale-gte17 * @endcap-width;
		marker-height: @endcap-scale-gte17 * @endcap-height;
	}
}

#intersections[nameEndAngle2!=null][connections=null]::cap2
{
	marker-file: url("sprites/cap.png");
	marker-allow-overlap: true;
	marker-placement: point; 
	
	[zoom<=15]
	{
		marker-transform: 'rotate([nameEndAngle2]) translate(0 ' + @endcap-offset-large-15 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle2]) translate(0 ' + @endcap-offset-small-15 + ')';}
	}
	
	[zoom>=16]
	{
		marker-transform: 'rotate([nameEndAngle2]) translate(0 ' + @endcap-offset-large-gte16 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle2]) translate(0 ' + @endcap-offset-small-gte16 + ')';}
	}
	
	[zoom<=14] 
	{ 
		marker-width: @endcap-scale-lte14 * @endcap-width;
		marker-height: @endcap-scale-lte14 * @endcap-height;
	}
	[zoom=15] 
	{ 
		marker-width: @endcap-scale-15 * @endcap-width;
		marker-height: @endcap-scale-15 * @endcap-height;
	}
	[zoom=16] 
	{ 
		marker-width: @endcap-scale-16 * @endcap-width;
		marker-height: @endcap-scale-16 * @endcap-height;
	}
	[zoom>=17] 
	{ 
		marker-width: @endcap-scale-gte17 * @endcap-width;
		marker-height: @endcap-scale-gte17 * @endcap-height;
	}
}

#intersections[nameEndAngle3!=null][connections=null]::cap3
{
	marker-file: url("sprites/cap.png");
	marker-allow-overlap: true;
	marker-placement: point; 
	marker-transform: 'rotate([nameEndAngle3]) translate(0 ' + @endcap-offset-large-gte16 + ')'; 
	[connections=2] {marker-transform: 'rotate([nameEndAngle3]) translate(0 ' + @endcap-offset-small-gte16 + ')';}
	
	[zoom<=15]
	{
		marker-transform: 'rotate([nameEndAngle3]) translate(0 ' + @endcap-offset-large-15 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle3]) translate(0 ' + @endcap-offset-small-15 + ')';}
	}
	
	[zoom>=16]
	{
		marker-transform: 'rotate([nameEndAngle3]) translate(0 ' + @endcap-offset-large-gte16 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle3]) translate(0 ' + @endcap-offset-small-gte16 + ')';}
	}
	
	[zoom<=14] 
	{ 
		marker-width: @endcap-scale-lte14 * @endcap-width;
		marker-height: @endcap-scale-lte14 * @endcap-height;
	}
	[zoom=15] 
	{ 
		marker-width: @endcap-scale-15 * @endcap-width;
		marker-height: @endcap-scale-15 * @endcap-height;
	}
	[zoom=16] 
	{ 
		marker-width: @endcap-scale-16 * @endcap-width;
		marker-height: @endcap-scale-16 * @endcap-height;
	}
	[zoom>=17] 
	{ 
		marker-width: @endcap-scale-gte17 * @endcap-width;
		marker-height: @endcap-scale-gte17 * @endcap-height;
	}
}

#intersections[nameEndAngle4!=null][connections=null]::cap4
{
	marker-file: url("sprites/cap.png");
	marker-allow-overlap: true;
	marker-placement: point; 
	
	[zoom<=15]
	{
		marker-transform: 'rotate([nameEndAngle4]) translate(0 ' + @endcap-offset-large-15 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle4]) translate(0 ' + @endcap-offset-small-15 + ')';}
	}
	
	[zoom>=16]
	{
		marker-transform: 'rotate([nameEndAngle4]) translate(0 ' + @endcap-offset-large-gte16 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle4]) translate(0 ' + @endcap-offset-small-gte16 + ')';}
	}
	
	[zoom<=14] 
	{ 
		marker-width: @endcap-scale-lte14 * @endcap-width;
		marker-height: @endcap-scale-lte14 * @endcap-height;
	}
	[zoom=15] 
	{ 
		marker-width: @endcap-scale-15 * @endcap-width;
		marker-height: @endcap-scale-15 * @endcap-height;
	}
	[zoom=16] 
	{ 
		marker-width: @endcap-scale-16 * @endcap-width;
		marker-height: @endcap-scale-16 * @endcap-height;
	}
	[zoom>=17] 
	{ 
		marker-width: @endcap-scale-gte17 * @endcap-width;
		marker-height: @endcap-scale-gte17 * @endcap-height;
	}
}

#intersections[nameEndAngle5!=null][connections=null]::cap5
{
	marker-file: url("sprites/cap.png");
	marker-allow-overlap: true;
	marker-placement: point; 
	
	[zoom<=15]
	{
		marker-transform: 'rotate([nameEndAngle5]) translate(0 ' + @endcap-offset-large-15 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle5]) translate(0 ' + @endcap-offset-small-15 + ')';}
	}
	
	[zoom>=16]
	{
		marker-transform: 'rotate([nameEndAngle5]) translate(0 ' + @endcap-offset-large-gte16 + ')'; 
		[connections=2] {marker-transform: 'rotate([nameEndAngle5]) translate(0 ' + @endcap-offset-small-gte16 + ')';}
	}
	
	[zoom<=14] 
	{ 
		marker-width: @endcap-scale-lte14 * @endcap-width;
		marker-height: @endcap-scale-lte14 * @endcap-height;
	}
	[zoom=15] 
	{ 
		marker-width: @endcap-scale-15 * @endcap-width;
		marker-height: @endcap-scale-15 * @endcap-height;
	}
	[zoom=16] 
	{ 
		marker-width: @endcap-scale-16 * @endcap-width;
		marker-height: @endcap-scale-16 * @endcap-height;
	}
	[zoom>=17] 
	{ 
		marker-width: @endcap-scale-gte17 * @endcap-width;
		marker-height: @endcap-scale-gte17 * @endcap-height;
	}
}