<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" 
		xmlns:h="http://schemas.autodesk.com/pseb/dm/DataImport/2015-04-14"  exclude-result-prefixes="h">
	<xsl:output method="text" indent="no"/>	
	<xsl:variable name="delimiter" select="';'"/>
	<!-- <xsl:key name="headentr" match="h:UDP" use="@Name"/> -->
	<xsl:template match="/">
	<xsl:text>ParentFileID;ChildFileID;IsAttachment;IsDependency;NeedsResolution;Source;RefId</xsl:text>
	<xsl:text>&#10;</xsl:text>
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="h:File">
		<xsl:apply-templates></xsl:apply-templates>
	</xsl:template>
	
	<xsl:template match="h:Revision">
		<xsl:apply-templates></xsl:apply-templates>
	</xsl:template>
	
	<xsl:template match="h:Iteration">		
		<xsl:apply-templates></xsl:apply-templates>
	</xsl:template>
	
	<xsl:template match="h:Association">
		<xsl:value-of select="concat(substring(parent::h:Iteration/@Id,2), $delimiter, substring(@ChildId,2), $delimiter)"/>
		<xsl:choose>
			<xsl:when test="@Type='Attachment'">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>			
		</xsl:choose>		
		<xsl:text>;</xsl:text>
		<xsl:choose>
			<xsl:when test="@Type='Dependency'">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>			
		</xsl:choose>
		<xsl:text>;</xsl:text>
		<xsl:choose>
			<xsl:when test="@NeedsResolution='true'">1</xsl:when>
			<xsl:otherwise>0</xsl:otherwise>			
		</xsl:choose>
		<xsl:text>;</xsl:text>
		<xsl:value-of select="concat(@Source, $delimiter, @RefId)"/>
		<xsl:text>&#10;</xsl:text>
	</xsl:template>
	<xsl:template match="text()"/>
</xsl:stylesheet>
