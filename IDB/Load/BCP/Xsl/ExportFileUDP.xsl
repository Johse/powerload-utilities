<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" 
		xmlns:h="http://schemas.autodesk.com/pseb/dm/DataImport/2015-04-14"  exclude-result-prefixes="h">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
	<xsl:key name="headentr" match="h:UDP" use="@Name"/>
	<xsl:template match="/">
		<list>
		<xsl:apply-templates/>
		</list>
	</xsl:template>
	<xsl:template match="h:File">
		<xsl:apply-templates></xsl:apply-templates>
	</xsl:template>
	
	<xsl:template match="h:Revision">
		<xsl:apply-templates></xsl:apply-templates>
	</xsl:template>
	
	<xsl:template match="h:Iteration">		
		<xsl:apply-templates select="h:UDP[count(.|key('headentr', @Name)[1]) = 1]"></xsl:apply-templates>
	</xsl:template>
	
	<xsl:template match="h:UDP">
		<UDP>
			<xsl:text>UDP_</xsl:text>
			<xsl:value-of select="@Name"/>
		</UDP>
	</xsl:template>
	<xsl:template match="text()"/>
		<xsl:template match="h:Folder/h:UDP"/>
</xsl:stylesheet>
