<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0" 
		xmlns:h="http://schemas.autodesk.com/pseb/dm/DataImport/2015-04-14"  exclude-result-prefixes="h">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
	<xsl:template match="/">
		<list>
		<xsl:apply-templates/>
		</list>
	</xsl:template>
	<xsl:template match="h:Behaviors">
		<xsl:apply-templates/>
	</xsl:template>
	<xsl:template match="h:PropertyDefinition[not(@Type)]">
		<xsl:apply-templates select="h:Assignment[@Class = 'Folder']"></xsl:apply-templates>
	</xsl:template>
	<xsl:template match="h:PropertyDefinition[@Type='DateTime']">
		<xsl:apply-templates select="h:Assignment[@Class = 'Folder']" mode="datetime"></xsl:apply-templates>
	</xsl:template>
	<xsl:template match="h:PropertyDefinition[@Type='Numeric']">
		<xsl:apply-templates select="h:Assignment[@Class = 'Folder']" mode="numeric"></xsl:apply-templates>
	</xsl:template>
	<xsl:template match="h:PropertyDefinition[@Type='Bool']">
		<xsl:apply-templates select="h:Assignment[@Class = 'Folder']"  mode="bool"></xsl:apply-templates>
	</xsl:template>
	<xsl:template match="h:Assignment[@Class = 'Folder']">
		<UDP DataType="nvarchar(MAX)">
			<xsl:text>UDP_</xsl:text>
			<xsl:value-of select="translate(../@Name, ' ','_')"/>
		</UDP>
	</xsl:template>
	<xsl:template match="h:Assignment[@Class = 'Folder']" mode="datetime">
		<UDP DataType="DateTime">
			<xsl:text>UDP_</xsl:text>
			<xsl:value-of select="translate(../@Name, ' ','_')"/>
		</UDP>
	</xsl:template>
	<xsl:template match="h:Assignment[@Class = 'Folder']" mode="numeric">
		<UDP DataType="Decimal(100,20)">
			<xsl:text>UDP_</xsl:text>
			<xsl:value-of select="translate(../@Name, ' ','_')"/>
		</UDP>
	</xsl:template>
	<xsl:template match="h:Assignment[@Class = 'Folder']" mode="bool">
		<UDP DataType="bit">
			<xsl:text>UDP_</xsl:text>
			<xsl:value-of select="translate(../@Name, ' ','_')"/>
		</UDP>
	</xsl:template>
	<xsl:template match="text()"/>
	<xsl:template match="h:LifecycleDefinition"/>
	<xsl:template match="h:RevisionDefinition"/>
</xsl:stylesheet>
