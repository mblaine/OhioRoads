# Preamble

This tutorial is not going to be totally user friendly. I did not record everything I did as I went. The best I could design I would compare to [Clark Griswold's garage outlet.](https://www.youtube.com/watch?v=iXaw70X7wb4)

I did all of this work on Windows 10 and using the Windows Subsystem for Linux running Ubuntu 22.04.4 LTS. I would recommend these resources for more information:

* https://switch2osm.org/serving-tiles/manually-building-a-tile-server-ubuntu-24-04-lts/
* https://www.openstreetmap.org/user/ZeLonewolf/diary/401697
* https://www.openstreetmap.org/user/SomeoneElse/diary/42617
* https://www.openstreetmap.org/user/ZeLonewolf/diary/402227

# Load data

With ohio-merged.osm.pbf in hand (see OhioRoadInventoryExtractor and BUILDING.txt in this repository), it can be loaded into postgresql with the following command and scripts:

    osm2pgsql -d gis --create --slim  -G --hstore --tag-transform-script ./TIMS.lua -C 2500 --number-processes 1 -S ./ohio.style /mnt/d/path/goes/here/ohio-merged.osm.pbf

# Configure Apache and Renderd

In /etc/apache2/conf-available/renderd.conf I set the following so the tiles are created outside of the virtual Linux installation, so as to not explode the size of the virtual hard disk file it lives in. This isn't necessary, I just needed to based on the disk space I had left.
    ModTileTileDir /mnt/d/TIMS/tile_cache
	
In  /etc/renderd.conf I also set:
    tile_dir=/mnt/d/TIMS/tile_cache

I had some trouble with sockets and the Apache service that I resolved by running:
    sudo chmod 777 /var/run/renderd

# Create the style

Run this to crate the style for Mapnik:

    carto project.mml > mapnik.xml

I then ran the following lines to do some finding and replacing in the output mapnik.xml, to take advantage of a couple of features that Mapnik supports but CartoCSS doesn't seem to.

One is using two different font sizes within one text label. And the other is selectively trying smaller and smaller fonts when text won't fit without overlapping otherwise.

    sed -i -E "s/<\!\[CDATA\[\[name\]\+'\\\\n'\+SIZE([0-9]+) ?\[ref\]\]\]>/[name]+'\\\\n'<Format size='\1'>\[ref\]<\/Format>/g" mapnik.xml
    sed -i -E "s/(placement-type=.list.+?)(<\/TextSymbolizer>)/\1<Placement wrap-width='30'>\[name\]<\/Placement><Placement size='12'>\[name\]<\/Placement><Placement size='11'>\[name\]<\/Placement><Placement size='10'>\[name\]<\/Placement><Placement size='9'>\[name\]<\/Placement><Placement size='8'>\[name\]<\/Placement>\2/g" mapnik.xml

# Render everything

I used [render_list_geo.pl](https://github.com/alx77/render_list_geo.pl) to force all of the tiles to be generated as mod_tile metatile files.
I also used render_list for the lower zooms since the script wasn't working for them for some reason. I may have been passing the wrong parameters.

For the record, for the state of Ohio, these zoom levels took this amount of time to fully render:

|zoom|time|
|----|----|
|12|0:00:20|
|13|0:00:32|
|14|0:01:34|
|15|0:03:08|
|16|0:13:06|
|17|0:31:56|
|18|1:40:05|
|19|5:32:49|

I decided to stop at zoom 19.

    render_list -a -m s2o -z 0 -Z 0 -x 0 -X 0 -y 0 -Y 0 -v -n 8
    render_list -a -m s2o -z 1 -Z 1 -x 0 -X 0 -y 0 -Y 0 -v -n 8
    render_list -a -m s2o -z 2 -Z 2 -x 1 -X 1 -y 1 -Y 1 -v -n 8
    render_list -a -m s2o -z 3 -Z 3 -x 2 -X 2 -y 2 -Y 3 -v -n 8
    ./render_list_geo.pl -a -m s2o -z 4 -Z 19 -x -84.821 -X -80.51 -y 38.4 -Y 41.98 -v -n 8

# Combine metatiles into one mbtiles file

I used [geofabrik/meta2tile](https://github.com/geofabrik/meta2tile) to convert the generated metatile files into one giant file. It took 24 hours and 17 minutes.

    ./meta2tile /mnt/d/TIMS/tile_cache/s2o/ /mnt/d/TIMS/tile_cache/final/combo.mbt --mbtiles

The output:

> Total for all tiles converted
> Meta tiles converted: Converted 896806 tiles in 87422.24 seconds (10.26 tiles/s)
> Total tiles converted: Converted 57395584 tiles in 87422.24 seconds (656.53 tiles/s)

# Convert to pmtiles

Finally I converted the mbtiles archive to [pmtiles.](https://github.com/protomaps/go-pmtiles/releases)

    pmtiles.exe convert combo.mbt ohio.pmtiles

This step took 3 hours 28 minutes. The file contained 57,395,413 tiles, 7,679,369 of them unique. So about 50 million of the individual png tiles were duplicates, most likely empty tiles at zoom 18 and 19 I guess.

The final ohio.pmtiles file is 33.2 GB.

One last step, I had to modify the [pmtiles file header](https://github.com/protomaps/PMTiles/blob/main/spec/v3/spec.md) to specify that all of the tiles are in png format. I set TT (Tile Type) to 0x02 using a [hex editor](https://mh-nexus.de/en/hxd/).

# Serving the tiles

I followed this guide to setting up a way to serve the tiles using AWS. The pmtiles file is stored using S3.

* https://docs.protomaps.com/deploy/aws

Their code for the Lambda function needs the byte in the pmtiles file header, set at the end of the previous section, for it to serve the tiles with content-type image/png.

I also decided to set the cache timeout to the number of seconds for 30 days instead of 1 since what I've created here should only need upating once a year.

So far it's been just less than a month. I don't know if anyone other than me is using this, but my bill has totaled to about $1.20. About half for storing the file in S3 and half for using Route 53 to serve the tiles with my custom domain and certificate. I had registered the domain with Cloudflare, but everything else came from Amazon including the cert.