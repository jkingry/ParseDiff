namespace ParseDiff.Tests
{

    using System;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

    [TestClass]
    public class TestEquality
    {
        [TestMethod]
        public void SameDiffShouldBeEqual()
        {
            var diff = @"
diff --git a/file b/file
index 123..456 789
--- a/file
+++ b/file
@@ -1,2 +1,2 @@
- line1
+ line2";

            var files1 = Diff.Parse(diff, Environment.NewLine).ToArray();
            var files2 = Diff.Parse(diff, Environment.NewLine).ToArray();

            AreEqual(files1.First(), files2.First());
            IsTrue(Enumerable.SequenceEqual(files1, files2));
        }


        [TestMethod]
        public void DifferentDiffShouldNotBeEqual()
        {
            var diff1 = @"
diff --git a/file b/file
index 123..456 789
--- a/file
+++ b/file
@@ -1,2 +1,2 @@
- line1
+ line2";

            var diff2 = @"
diff -r 514fc757521e lib/parsers.coffee
--- a/lib/parsers.coffee	Thu Jul 09 00:56:36 2015 +0200
+++ b/lib/parsers.coffee	Fri Jul 10 16:23:43 2015 +0200
@@ -43,6 +43,9 @@
             files[file] = { added: added, deleted: deleted }
         files
+    diff: (out) ->
+        files = {}
+
 module.exports = Parsers
 module.exports.version = (out) ->
";
            var files1 = Diff.Parse(diff1, Environment.NewLine).ToArray();
            var files2 = Diff.Parse(diff2, Environment.NewLine).ToArray();

            AreNotEqual(files1.First(), files2.First());
            IsFalse(Enumerable.SequenceEqual(files1, files2));
        }

        [TestMethod]
        public void DifferentChunksDiffShouldNotBeEqual()
        {
            var diff1 = @"
diff -r 514fc757521e lib/parsers.coffee
--- a/lib/parsers.coffee	Thu Jul 09 00:56:36 2015 +0200
+++ b/lib/parsers.coffee	Fri Jul 10 16:23:43 2015 +0200
@@ -43,6 +43,9 @@
             files[file] = { added: added, deleted: deleted }
         files
+    diff: (out) ->
+        files = {}
+
 module.exports = Parsers
 module.exports.version = (out) ->
";

            var diff2 = @"
diff -r 514fc757521e lib/parsers.coffee
--- a/lib/parsers.coffee	Thu Jul 09 00:56:36 2015 +0200
+++ b/lib/parsers.coffee	Fri Jul 10 16:23:43 2015 +0200
@@ -43,6 +43,9 @@
             files[file] = { added: added, deleted: deleted }
         files
+    diff: (out) ->
+  XXX   files = {}
+
 module.exports = Parsers
 module.exports.version = (out) ->
";
            var files1 = Diff.Parse(diff1, Environment.NewLine).ToArray();
            var files2 = Diff.Parse(diff2, Environment.NewLine).ToArray();

            AreNotEqual(files1.First(), files2.First());
            IsFalse(Enumerable.SequenceEqual(files1, files2));
        }

        [TestMethod]
        public void RebasedPatchesShouldBeEqual()
        {
            var patch1 = @"
# HG changeset patch
# User xxx
# Date 1483371015 -3600
#      Mon Jan 02 16:30:15 2017 +0100
# Node ID e5c9a138e019a6c2851c2bcd7960046b65c0fa9f
# Parent  b612ff73463ca19be65c7f1235c275bd011a9feb
Change 111

diff -r b612ff73463c -r e5c9a138e019 1.txt
--- a/1.txt	Thu Dec 22 14:58:46 2016 +0100
+++ b/1.txt	Mon Jan 02 16:30:15 2017 +0100
@@ -1,5 +1,5 @@
 qwesfsafadsadsadsadsadsad
-zxcxzczxcxc
+zxCHANGEzxcxc
 
 
xxx
";
            var patch2 = @"
# HG changeset patch
# User xxx
# Date 1483371015 -3600
#      Mon Jan 02 16:30:15 2017 +0100
# Node ID 43339e170990fe2873b2866d5d916b6ec3ae0956
# Parent  4afd4b93aa45455be267d5e4541094daea9b02f3
Change 111

diff -r 4afd4b93aa45 -r 43339e170990 1.txt
--- a/1.txt	Thu Dec 22 14:59:09 2016 +0100
+++ b/1.txt	Mon Jan 02 16:30:15 2017 +0100
@@ -1,5 +1,5 @@
 qwesfsafadsadsadsadsadsad
-zxcxzczxcxc
+zxCHANGEzxcxc
 
 
xxx
";

            var files1 = Diff.Parse(patch1).ToArray();
            var files2 = Diff.Parse(patch2).ToArray();

            AreEqual(files1.First(), files2.First());
            IsTrue(Enumerable.SequenceEqual(files1, files2));
        }

            [TestMethod]
        public void DifferentIndexesShouldNotBeEqual()
        {
            var diffWithIndexLine = @"
diff --git a/file b/file
index 123..456 789
--- a/file
+++ b/file
@@ -1,2 +1,2 @@
- line1
+ line2";
            var diffWithNoIndexLine = @"
diff --git a/file b/file
--- a/file
+++ b/file
@@ -1,2 +1,2 @@
- line1
+ line2";

            var files1 = Diff.Parse(diffWithIndexLine, Environment.NewLine).ToArray();
            var files2 = Diff.Parse(diffWithNoIndexLine, Environment.NewLine).ToArray();

            AreNotEqual(files1.First(), files2.First());
            AreNotEqual(files2.First(), files1.First());
            IsFalse(Enumerable.SequenceEqual(files1, files2));
        }
    }
}
