<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    version="1.0"
    xmlns:h="http://schemas.autodesk.com/pseb/dm/DataImport/2015-04-14">
  <xsl:output method="text" indent="no"/>
<xsl:variable name="delimiter" select="';'"/>
  <xsl:template match="/">
	<xsl:text>FolderID;ParentFolderId;Name;Category;User;CreateDate;Path</xsl:text>
      <xsl:text>&#10;</xsl:text>
      <xsl:apply-templates select="descendant::h:Folder"/>
  </xsl:template>
  <xsl:template match="h:Behaviors | h:Security | h:Statistics">
  </xsl:template>
  <xsl:template match="h:Folder">
  <xsl:value-of select="concat($delimiter, $delimiter, @Name, $delimiter, @Category, $delimiter, h:Created/@User, $delimiter, h:Created/@Date)"/>
  <xsl:text>;$/</xsl:text>
	<xsl:apply-templates select="ancestor-or-self::h:Folder/@Name"/>	
      <xsl:text>&#10;</xsl:text>	  
  </xsl:template>
  <xsl:template match="@Name">
      <xsl:if test="position() > 1">/</xsl:if>
      <xsl:value-of select="."/>
  </xsl:template>
</xsl:stylesheet>
